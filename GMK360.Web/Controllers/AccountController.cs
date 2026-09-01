using System.Security.Claims;
using System;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Core.Extensions;
using GMK360.Data.Contexts;
using GMK360.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using GMK360.Web.Services.Sms;

namespace GMK360.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UI.Services.IEmailSender _emailSender;
        private readonly GMK360.Web.Services.WalletService _walletService;
        private readonly ISmsService _smsService;
        private readonly GMK360.Core.Services.INviValidationService _nviValidationService;

        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            ApplicationDbContext context, 
            Microsoft.AspNetCore.Identity.UI.Services.IEmailSender emailSender,
            GMK360.Web.Services.WalletService walletService,
            ISmsService smsService,
            GMK360.Core.Services.INviValidationService nviValidationService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailSender = emailSender;
            _walletService = walletService;
            _smsService = smsService;
            _nviValidationService = nviValidationService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (string.IsNullOrEmpty(model.Email) && string.IsNullOrEmpty(model.PhoneNumber))
            {
                ModelState.AddModelError(string.Empty, "Lütfen E-Posta adresinizi veya Telefon numaranızı giriniz.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                ApplicationUser user = null;
                if (!string.IsNullOrEmpty(model.Email))
                {
                    user = await _userManager.FindByEmailAsync(model.Email);
                }
                else if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == model.PhoneNumber);
                }

                if (user != null)
                {
                    var subdomain = HttpContext.Items["Subdomain"] as string;
                    if (!string.IsNullOrEmpty(subdomain))
                    {
                        var hasAccess = await _context.AgencyConsultants
                            .AnyAsync(ac => ac.UserId == user.Id && ac.Agency.Subdomain == subdomain && ac.IsActive);
                        
                        if (!hasAccess && !await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            ModelState.AddModelError(string.Empty, "Bu kurumsal panele giriş yetkiniz bulunmuyor.");
                            return View(model);
                        }
                    }

                    var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                    if (isPasswordValid)
                    {
                        // DEV: Cihaz doğrulamasını şimdilik devre dışı bırakıyoruz ki hızlıca test edebilelim
                        // var deviceCookie = Request.Cookies["DeviceFingerprint_" + user.Id];
                        // if (string.IsNullOrEmpty(deviceCookie))
                        // {
                        //     TempData["VerifyUserId"] = user.Id;
                        //     TempData["VerifyRememberMe"] = model.RememberMe;
                        //     return RedirectToAction("VerifyDevice", new { returnUrl });
                        // }
                    }

                    // GÜVENİLİR CİHAZ VEYA HATA DURUMLARI (Normal Login Akışı)
                    // Kullanıcı bulunduysa, her zaman UserName ile login dene (Email veya Phone yerine UserName)
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
                    
                                    if (result.Succeeded)
                {

                        // Check if the user is an Admin or SuperAdmin FIRST
                        if (await _userManager.IsInRoleAsync(user, "SuperAdmin") || await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        
                        // Akıllı Yönlendirme ve Kaldığı Yerden Devam Etme (Sihirbaz)
                        if (user.UserType == UserType.Individual || user.UserType == UserType.PropertyOwner)
                        {
                            // Eğer kullanıcının henüz hiç bir "Evi" (Property) yoksa, sihirbazı tamamlamamış demektir.
                            bool hasCompletedWizard = _context.Properties.Any(p => p.UserId == user.Id);
                            if (!hasCompletedWizard)
                            {
                                return RedirectToAction("SetupWizard", "DigitalHome");
                            }
                        }
                        else if (user.UserType == UserType.ServiceProvider)
                        {
                            // Usta sihirbaz kontrolü (ileride eklenecek)
                        }
                        
                        return RedirectToAction("Index", "CustomerDashboard");
                    }
                    
                    if (result.IsNotAllowed)
                    {
                        ModelState.AddModelError(string.Empty, "Hesabınız henüz doğrulanmamış. Lütfen e-posta adresinize gönderilen linke tıklayarak hesabınızı aktifleştirin.");
                        return View(model);
                    }
                    
                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "Çok fazla hatalı deneme yaptınız. Hesabınız güvenlik amacıyla 15 dakika kilitlenmiştir.");
                        return View(model);
                    }
                }

                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi. Bilgiler hatalı.");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LoginAjax([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                                if (result.Succeeded)
                {

                    return Json(new { success = true });
                }
                if (result.IsNotAllowed)
                {
                    return Json(new { success = false, message = "Lütfen önce e-posta adresinize gönderilen onay linkine tıklayın." });
                }
                if (result.IsLockedOut)
                {
                    return Json(new { success = false, message = "Çok fazla hatalı deneme yaptınız. Hesabınız 15 dakika kilitlenmiştir." });
                }
            }
            return Json(new { success = false, message = "E-posta veya şifre hatalı." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken] public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        
        [HttpGet]
        public IActionResult Register([FromQuery(Name = "ref")] string? referralCode, [FromQuery(Name = "token")] string? shadowToken)
        {
            var model = new RegisterViewModel();
            if (!string.IsNullOrEmpty(referralCode))
            {
                model.ReferralCode = referralCode;
            }
            if (!string.IsNullOrEmpty(shadowToken))
            {
                model.ShadowToken = shadowToken;
                // If shadow token exists, user is forced to be Individual (they are just a shadow of another account)
                model.UserType = UserType.Individual;
            }
            
            ViewBag.ServiceCategories = _context.DefinitionValues
                .Where(dv => dv.Category.SystemCode == "SERVICE_CATEGORY")
                .OrderBy(dv => dv.Name).ToList();

            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewBag.ServiceCategories = _context.DefinitionValues
                .Where(dv => dv.Category.SystemCode == "SERVICE_CATEGORY")
                .OrderBy(dv => dv.Name).ToList();

            if (ModelState.IsValid)
            {
                // 1. NVI DOĞRULAMASI
                bool isNviValid = true;
                if (!string.IsNullOrEmpty(model.TcIdentityNo) && model.DateOfBirth.HasValue)
                {
                    isNviValid = await _nviValidationService.ValidateTcIdentityAsync(
                        model.TcIdentityNo, 
                        model.FirstName, 
                        model.LastName, 
                        model.DateOfBirth.Value.Year
                    );
                }

                if (!isNviValid)
                {
                    ModelState.AddModelError("TcIdentityNo", "MERNİS (NVI) doğrulaması başarısız oldu. Girdiğiniz bilgileri kontrol ediniz.");
                    return View(model);
                }

                model.FirstName = model.FirstName.ToUpper(new System.Globalization.CultureInfo("tr-TR"));
                model.LastName = model.LastName.ToUpper(new System.Globalization.CultureInfo("tr-TR"));
                model.Email = model.Email.ToLower(new System.Globalization.CultureInfo("en-US"));
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    TcIdentityNo = model.TcIdentityNo,
                    BirthYear = model.DateOfBirth?.Year,
                    DisplayName = $"{model.FirstName} {model.LastName}",
                    ProfileSlug = await GenerateUniqueProfileSlugAsync(model.Email.Split('@')[0]),
                    UserType = model.UserType,
                    PhoneNumberConfirmed = false, // Gecikmeli doğrulama
                    EmailConfirmed = false, // Gecikmeli doğrulama
                    ReferralCode = "GMK-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(), // Kendisinin referans kodu
                    RegisteredCampaignId = model.CampaignId
                };

                // 2. REFERANS KODU (ref) İLE GELMİŞSE
                if (!string.IsNullOrEmpty(model.ReferralCode))
                {
                    var referrer = await _userManager.Users.FirstOrDefaultAsync(u => u.ReferralCode == model.ReferralCode);
                    if (referrer != null)
                    {
                        user.ReferredByUserId = referrer.Id;
                    }
                }

                // 3. GÖLGE KULLANICISI (Davet/Token) İLE GELMİŞSE
                if (!string.IsNullOrEmpty(model.ShadowToken))
                {
                    // Token çözme mantığı burada (Örn: veritabanında PendingInvites gibi bir tabloda aranır veya token JWT'dir)
                    // Şimdilik token geçerli varsayıyoruz ve rastgele bir adminin altına atıyoruz (Demo amaçlı)
                    var admin = await _userManager.Users.FirstOrDefaultAsync(u => u.UserType == UserType.Corporate);
                    if (admin != null)
                    {
                        user.ShadowCreatorId = admin.Id;
                        user.IsShadowAccount = true;
                    }
                }

                var result = await _userManager.CreateAsync(user, model.Password);
                
                                if (result.Succeeded)
                {

                    // 4. KURUMSAL VEYA USTA İSE EK TABLOLARA KAYIT
                    if (user.UserType == UserType.Corporate)
                    {
                        var corp = new CorporateProfile
                        {
                            ApplicationUserId = user.Id,
                            CompanyName = model.CompanyName ?? $"{model.FirstName} Şirketi",
                            TaxNumber = model.TaxNumber,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.CorporateProfiles.Add(corp);
                    }
                    else if (user.UserType == UserType.ServiceProvider)
                    {
                        var provider = new GMK360.Core.Entities.ServiceProvider
                        {
                            UserId = user.Id,
                            BusinessName = $"{model.FirstName} {model.LastName}",
                            ReliabilityScore = 100,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.ServiceProviders.Add(provider);
                        
                        // Hizmet Kategorisi eklenebilir
                        // ...
                    }

                    await _context.SaveChangesAsync();

                    // Ücretsiz Başlangıç Aboneliği Ekle (Kullanıcı Tipi Bireysel/Yatırımcı/Oturan ise)
                    if (user.UserType == UserType.Individual || user.UserType == UserType.PropertyOwner)
                    {
                        var freePackage = await _context.SubscriptionPackages.FirstOrDefaultAsync(p => p.Period == PackagePeriod.Free);
                        if (freePackage == null)
                        {
                            freePackage = new SubscriptionPackage
                            {
                                Name = "Dijital Evim - Ücretsiz Başlangıç",
                                Period = PackagePeriod.Free,
                                Price = 0,
                                TargetUserType = UserType.Individual,
                                MaxPropertiesCount = 1,
                                MaxRentTrackingCount = 3,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.SubscriptionPackages.Add(freePackage);
                            await _context.SaveChangesAsync();
                        }
                        
                        var sub = new UserSubscription
                        {
                            UserId = user.Id,
                            PackageId = freePackage.Id,
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddDays(30), // 30 Günlük Deneme Sürümü
                            IsActive = true,
                            PricePaid = 0,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.UserSubscriptions.Add(sub);
                        await _context.SaveChangesAsync();
                    }

                    // Giriş Yap
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    // Akıllı Sihirbaz (Wizard) Yönlendirmesi
                    if (user.UserType == UserType.Individual || user.UserType == UserType.PropertyOwner)
                    {
                        // Dijital Evim kurulum sihirbazına git
                        return RedirectToAction("SetupWizard", "DigitalHome");
                    }
                    else if (user.UserType == UserType.ServiceProvider)
                    {
                        // Usta profil kurulum sihirbazına git
                        return RedirectToAction("SetupWizard", "Usta");
                    }
                    else if (user.UserType == UserType.Corporate)
                    {
                        return RedirectToAction("SetupWizard", "Corporate");
                    }

                    // Varsayılan
                    return RedirectToAction("Index", "CustomerDashboard");
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyPhone()
        {
            if (TempData["UserId"] == null)
            {
                return RedirectToAction("Login");
            }
            TempData.Keep("UserId");
            TempData.Keep("SmsOtpCode");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhone(string code)
        {
            var expectedCode = TempData["SmsOtpCode"]?.ToString();
            var userId = TempData["UserId"]?.ToString();

            if (string.IsNullOrEmpty(expectedCode) || string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            if (code == expectedCode)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.PhoneNumberConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    
                    // Referans hesaplamalarını yap
                    if (!string.IsNullOrEmpty(user.ReferredByUserId))
                    {
                        var referrer = await _userManager.FindByIdAsync(user.ReferredByUserId);
                        if (referrer != null)
                        {
                            await _walletService.AddGiftCreditAsync(referrer.Id, 25m, 30, $"Referans Ödülü (Yeni Kullanıcı: {user.FirstName})");
                            await _walletService.AddGiftCreditAsync(user.Id, 50m, 30, "Hoşgeldin Hediyesi");
                        }
                    }
                    else 
                    {
                        await _walletService.AddGiftCreditAsync(user.Id, 50m, 30, "Hoşgeldin Hediyesi");
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Dashboard");
                }
            }

            ModelState.AddModelError(string.Empty, "Geçersiz doğrulama kodu.");
            TempData.Keep("UserId");
            TempData.Keep("SmsOtpCode");
            return View();
        }

        private string GenerateRandomReferralCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return "GMK-" + new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Geçersiz kullanıcı ID'si: '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
                            if (result.Succeeded)
                {

                TempData["SuccessMessage"] = "E-posta adresiniz başarıyla doğrulandı. Şimdi giriş yapabilirsiniz!";
                return RedirectToAction("Login", "Account");
            }

            TempData["ErrorMessage"] = "E-posta doğrulama işlemi başarısız oldu.";
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> VerifyDevice(string returnUrl = null)
        {
            var userId = TempData.Peek("VerifyUserId")?.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Email"] = HideEmail(user.Email);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendDeviceCode(string provider, string returnUrl = null)
        {
            var userId = TempData.Peek("VerifyUserId")?.ToString();
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return RedirectToAction("Login");

            // Test ortamı için kodu her zaman 123456 yapıyoruz. Gerçekte Random kullanılmalı.
            var code = "123456"; // new Random().Next(100000, 999999).ToString();
            TempData["GeneratedDeviceCode"] = code;

            if (provider == "Email")
            {
                await _emailSender.SendEmailAsync(user.Email, "GMK360 - Yeni Cihaz Doğrulama Kodu", $"Güvenlik kodunuz: {code}");
                TempData["VerificationMessage"] = "Doğrulama kodu e-posta adresinize gönderildi.";
            }
            else if (provider == "SMS")
            {
                await _smsService.SmsGonderAsync(user.PhoneNumber, $"GMK360 cihaz doğrulama kodunuz: {code}");
                TempData["VerificationMessage"] = "Doğrulama kodu SMS ile telefonunuza gönderildi.";
            }

            return RedirectToAction("VerifyDeviceCode", new { returnUrl });
        }

        [HttpGet]
        public IActionResult VerifyDeviceCode(string returnUrl = null)
        {
            var userId = TempData.Peek("VerifyUserId")?.ToString();
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");
            
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyDeviceCode(string Code, string returnUrl = null)
        {
            var expectedCode = TempData.Peek("GeneratedDeviceCode")?.ToString();
            var userId = TempData.Peek("VerifyUserId")?.ToString();
            
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(expectedCode))
            {
                return RedirectToAction("Login");
            }

            if (Code == expectedCode)
            {
                var user = await _userManager.FindByIdAsync(userId);
                bool rememberMe = TempData["VerifyRememberMe"] as bool? ?? false;

                // Giriş yap
                await _signInManager.SignInAsync(user, isPersistent: rememberMe);

                // Cihazı güvenilir olarak işaretle (1 yıllık çerez)
                var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1),
                    HttpOnly = true,
                    Secure = true
                };
                Response.Cookies.Append("DeviceFingerprint_" + user.Id, Guid.NewGuid().ToString(), cookieOptions);

                // Temizlik
                TempData.Remove("VerifyUserId");
                TempData.Remove("GeneratedDeviceCode");
                TempData.Remove("VerifyRememberMe");

                // "Bu sen miydin?" E-postası Gönder (Arka planda)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailSender.SendEmailAsync(user.Email, "GMK360 - Yeni Cihaz Girişi", 
                            "Hesabınıza yeni bir cihazdan giriş yapıldı. Eğer bu siz değilseniz hemen şifrenizi değiştirin.");
                    }
                    catch { }
                });

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Girdiğiniz kod hatalı.");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> VerificationRequired()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (user.PhoneNumberConfirmed && user.IsEDevletVerified)
            {
                return RedirectToAction("Create", "Property");
            }

            return View(user);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public IActionResult RegisterCorporate()
        {
            return View(new RegisterCorporateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCorporate(RegisterCorporateViewModel model, [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // VKN Modül 10 Doğrulaması
            if (!ValidateVkn(model.TaxNumber))
            {
                ModelState.AddModelError("TaxNumber", "Girdiğiniz Vergi Kimlik Numarası (VKN) geçersizdir.");
                return View(model);
            }

            // NVI (KPSPublic) TCKN Doğrulaması (Yetkili Kişi İçin)
            var isNviValid = await _nviValidationService.ValidateTcIdentityAsync(
                model.TcIdentityNo, 
                model.FirstName, 
                model.LastName, 
                model.BirthYear
            );

            if (!isNviValid)
            {
                ModelState.AddModelError("TcIdentityNo", "NVI (Kimlik) doğrulaması başarısız oldu. Lütfen bilgilerinizi kontrol ediniz.");
                return View(model);
            }

            // Subdomain unique validation
            var existingAgency = await _context.Agencies.FirstOrDefaultAsync(a => a.Subdomain == model.Subdomain);
            if (existingAgency != null)
            {
                ModelState.AddModelError("Subdomain", "Bu alt alan adı (subdomain) zaten kullanılıyor. Lütfen başka bir tane seçiniz.");
                return View(model);
            }

            var displayName = model.CompanyName;
            model.FirstName = model.FirstName.ToUpper(new System.Globalization.CultureInfo("tr-TR"));
            model.LastName = model.LastName.ToUpper(new System.Globalization.CultureInfo("tr-TR"));
            model.Email = model.Email.ToLower(new System.Globalization.CultureInfo("en-US"));
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FirstName = model.FirstName, // Yetkili adı
                LastName = model.LastName, // Yetkili soyadı
                DisplayName = displayName,
                ProfileSlug = await GenerateUniqueProfileSlugAsync(displayName),
                UserType = UserType.Corporate
            };

            var result = await _userManager.CreateAsync(user, model.Password);
                            if (result.Succeeded)
                {

                var agency = new Agency
                {
                    CompanyName = model.CompanyName,
                    TaxNumber = model.TaxNumber,
                    Subdomain = model.Subdomain,
                    ThemePrimaryColor = model.ThemePrimaryColor,
                    ThemeSecondaryColor = "#ffffff",
                    
                    
                    ContractStartDate = System.DateTime.UtcNow,
                    ContractEndDate = System.DateTime.UtcNow.AddYears(1)
                };

                if (model.LogoFile != null && model.LogoFile.Length > 0)
                {
                    var uploadsFolder = System.IO.Path.Combine(env.WebRootPath, "uploads", "agencies");
                    System.IO.Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = System.Guid.NewGuid().ToString() + "_" + model.LogoFile.FileName;
                    var filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        await model.LogoFile.CopyToAsync(fileStream);
                    }
                    agency.LogoUrl = "/uploads/agencies/" + uniqueFileName;
                }

                _context.Agencies.Add(agency);
                await _context.SaveChangesAsync();

                var consultant = new AgencyConsultant
                {
                    AgencyId = agency.Id,
                    UserId = user.Id
                };
                _context.AgencyConsultants.Add(consultant);
                
                // Ana Çatı Kurumsal Profilini Oluştur
                var corporateProfile = new CorporateProfile
                {
                    ApplicationUserId = user.Id,
                    CompanyName = model.CompanyName,
                    TaxOffice = model.TaxOffice ?? "Belirtilmedi",
                    TaxNumber = model.TaxNumber
                };
                _context.CorporateProfiles.Add(corporateProfile);

                await _context.SaveChangesAsync();
                
                // Telefon doğrulaması (OTP) için OTP oluştur
                var otp = new Random().Next(100000, 999999).ToString();
                TempData["SmsOtpCode"] = otp; // Gerçek senaryoda veritabanı veya Redis'te tutulur
                TempData["UserId"] = user.Id;

                string smsMsg = $"GMK360 Kurumsal doğrulama kodunuz: {otp}. Kimseyle paylaşmayınız.";
                await _smsService.SmsGonderAsync(user.PhoneNumber, smsMsg);

                // Kayıt tamam, OTP doğrulama ekranına yönlendir.
                return RedirectToAction("VerifyPhone");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("~/Views/Home/KurumsalFirmaKatilim.cshtml", model);
        }

        // VKN Modül 10 Doğrulama Algoritması
        private bool ValidateVkn(string vkn)
        {
            if (string.IsNullOrEmpty(vkn) || vkn.Length != 10 || !long.TryParse(vkn, out _)) 
                return false;
            
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                int digit = Convert.ToInt32(vkn[i].ToString());
                int tmp = (digit + (9 - i)) % 10;
                if (tmp != 0)
                {
                    int pow = (int)(tmp * Math.Pow(2, 9 - i)) % 9;
                    sum += (pow != 0) ? pow : 9;
                }
            }
            int lastDigit = Convert.ToInt32(vkn[9].ToString());
            int result = (10 - (sum % 10)) % 10;
            
            return lastDigit == result;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                TempData["ErrorMessage"] = $"Dış sağlayıcıdan hata: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null && (!existingUser.IsEDevletVerified || !existingUser.PhoneNumberConfirmed))
                {
                    return RedirectToAction("Onboarding", "Account");
                }
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction("Lockout");
            }
            else
            {
                // Yeni kayıt
                var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
                if (email != null)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        var firstName = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.GivenName) ?? "Google";
                        var lastName = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Surname) ?? "Kullanıcısı";
                        var displayName = $"{firstName} {lastName}";
                        
                        user = new ApplicationUser 
                        { 
                            UserName = email, 
                            Email = email, 
                            EmailConfirmed = true,
                            FirstName = firstName,
                            LastName = lastName,
                            DisplayName = displayName,
                            ProfileSlug = await GenerateUniqueProfileSlugAsync(displayName),
                            UserType = GMK360.Core.Entities.Identity.UserType.Individual,
                            ReferralCode = GenerateRandomReferralCode(),
                            
                        };
                        var createResult = await _userManager.CreateAsync(user);
                        if (createResult.Succeeded)
                        {
                            await _userManager.AddLoginAsync(user, info);
                            await _signInManager.SignInAsync(user, isPersistent: false);
                                // Onboarding kontrolÃ¼
                                if (!user.IsEDevletVerified || !user.PhoneNumberConfirmed)
                                {
                                    return RedirectToAction("Onboarding", "Account");
                                }
                                return RedirectToLocal(returnUrl);
                        }
                    }
                    else
                    {
                        // Var olan maile Google ekle
                        await _userManager.AddLoginAsync(user, info);
                        await _signInManager.SignInAsync(user, isPersistent: false);
                                // Onboarding kontrolÃ¼
                                if (!user.IsEDevletVerified || !user.PhoneNumberConfirmed)
                                {
                                    return RedirectToAction("Onboarding", "Account");
                                }
                                return RedirectToLocal(returnUrl);
                    }
                }
                TempData["ErrorMessage"] = "Google girişi başarısız oldu.";
                return RedirectToAction(nameof(Login));
            }
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Onboarding()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (await _userManager.IsInRoleAsync(user, "SuperAdmin") || await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            // Eğer zaten onboarding tamamlandıysa
            if (user.PhoneNumberConfirmed && user.IsEDevletVerified)
            {
                return RedirectToAction("Index", "CustomerDashboard");
            }

            var model = new OnboardingViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                TcIdentityNo = user.TcIdentityNo,
                IsShadowAccount = user.IsShadowAccount
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onboarding(OnboardingViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

                        // NVI Doğrulaması
            var isNviValid = await _nviValidationService.ValidateTcIdentityAsync(
                model.TcIdentityNo,
                model.FirstName,
                model.LastName,
                model.BirthYear
            );

            if (!isNviValid)
            {
                ModelState.AddModelError(string.Empty, "TC Kimlik numarası doğrulaması başarısız oldu. Lütfen bilgilerinizi (Ad, Soyad, TC, Doğum Yılı) eksiksiz ve kimlikte yazdığı gibi giriniz.");
                return View(model);
            }

            // TODO: SMS OTP Entegrasyonu (Şimdilik doğrudan onaylıyoruz)
            user.PhoneNumber = model.PhoneNumber;
            user.PhoneNumberConfirmed = true; // SMS kodu onaylanınca true yapılmalı normalde

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.TcIdentityNo = model.TcIdentityNo;
            user.BirthYear = model.BirthYear;
            user.IsEDevletVerified = true;

            // Seçilen Kullanıcı Tipini Ata
            if (Enum.TryParse<GMK360.Core.Entities.Identity.UserType>(model.SelectedUserType, out var userType))
            {
                user.UserType = userType;
            }

            if (user.IsShadowAccount)
            {
                user.IsShadowAccount = false;
            }

            await _userManager.UpdateAsync(user);

            // Giriş çerezini tazele
            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Hub", "Dashboard");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        
        private async Task<string> GenerateUniqueProfileSlugAsync(string displayName)
        {
            if (string.IsNullOrEmpty(displayName)) return Guid.NewGuid().ToString("N").Substring(0, 8);
            
            var baseSlug = displayName.ToUrlSlug();
            var finalSlug = baseSlug;
            var suffix = 1;
            
            while (await _userManager.Users.AnyAsync(u => u.ProfileSlug == finalSlug))
            {
                finalSlug = $"{baseSlug}-{suffix}";
                suffix++;
            }
            
            return finalSlug;
        }

        private string HideEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return string.Empty;
            var parts = email.Split('@');
            if (parts.Length != 2) return email;
            var name = parts[0];
            if (name.Length > 2)
            {
                name = name.Substring(0, 2) + new string('*', name.Length - 2);
            }
            return $"{name}@{parts[1]}";
        }
    }
}











