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
                ModelState.AddModelError(string.Empty, "LÃ¼tfen E-Posta adresinizi veya Telefon numaranÄ±zÄ± giriniz.");
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
                            ModelState.AddModelError(string.Empty, "Bu kurumsal panele giriÅŸ yetkiniz bulunmuyor.");
                            return View(model);
                        }
                    }

                    var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                    if (isPasswordValid)
                    {
                        // DEV: Cihaz doÄŸrulamasÄ±nÄ± ÅŸimdilik devre dÄ±ÅŸÄ± bÄ±rakÄ±yoruz ki hÄ±zlÄ±ca test edebilelim
                        // var deviceCookie = Request.Cookies["DeviceFingerprint_" + user.Id];
                        // if (string.IsNullOrEmpty(deviceCookie))
                        // {
                        //     TempData["VerifyUserId"] = user.Id;
                        //     TempData["VerifyRememberMe"] = model.RememberMe;
                        //     return RedirectToAction("VerifyDevice", new { returnUrl });
                        // }
                    }

                    // GÃœVENÄ°LÄ°R CÄ°HAZ VEYA HATA DURUMLARI (Normal Login AkÄ±ÅŸÄ±)
                    // KullanÄ±cÄ± bulunduysa, her zaman UserName ile login dene (Email veya Phone yerine UserName)
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
                    
                                    if (result.Succeeded)
                {

                        // Check if the user is an Admin or SuperAdmin FIRST
                        if (await _userManager.IsInRoleAsync(user, "SuperAdmin") || await _userManager.IsInRoleAsync(user, "Admin")) { return RedirectToAction("Index", "Admin"); }
                        
                        if (await _userManager.IsInRoleAsync(user, "InsaatFirmasi")) { return RedirectToAction("Construction", "Dashboard"); }
                        if (await _userManager.IsInRoleAsync(user, "Corporate")) { return RedirectToAction("Corporate", "Dashboard"); }
                        
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        
                        // AkÄ±llÄ± YÃ¶nlendirme ve KaldÄ±ÄŸÄ± Yerden Devam Etme (Sihirbaz)
                        if (user.UserType == UserType.Individual || user.UserType == UserType.PropertyOwner)
                        {
                            // EÄŸer kullanÄ±cÄ±nÄ±n henÃ¼z hiÃ§ bir "Evi" (Property) yoksa, sihirbazÄ± tamamlamamÄ±ÅŸ demektir.
                            bool hasCompletedWizard = _context.Properties.Any(p => p.UserId == user.Id);
                            if (!hasCompletedWizard)
                            {
                                return RedirectToAction("SetupWizard", "DigitalHome");
                            }
                        }
                        else if (user.UserType == UserType.ServiceProvider)
                        {
                            // Usta sihirbaz kontrolÃ¼ (ileride eklenecek)
                        }
                        
                        return RedirectToAction("PropertyOwner", "Dashboard");
                    }
                    
                    if (result.IsNotAllowed)
                    {
                        ModelState.AddModelError(string.Empty, "HesabÄ±nÄ±z henÃ¼z doÄŸrulanmamÄ±ÅŸ. LÃ¼tfen e-posta adresinize gÃ¶nderilen linke tÄ±klayarak hesabÄ±nÄ±zÄ± aktifleÅŸtirin.");
                        return View(model);
                    }
                    
                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "Ã‡ok fazla hatalÄ± deneme yaptÄ±nÄ±z. HesabÄ±nÄ±z gÃ¼venlik amacÄ±yla 15 dakika kilitlenmiÅŸtir.");
                        return View(model);
                    }
                }

                ModelState.AddModelError(string.Empty, "GeÃ§ersiz giriÅŸ denemesi. Bilgiler hatalÄ±.");
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
                    return Json(new { success = false, message = "LÃ¼tfen Ã¶nce e-posta adresinize gÃ¶nderilen onay linkine tÄ±klayÄ±n." });
                }
                if (result.IsLockedOut)
                {
                    return Json(new { success = false, message = "Ã‡ok fazla hatalÄ± deneme yaptÄ±nÄ±z. HesabÄ±nÄ±z 15 dakika kilitlenmiÅŸtir." });
                }
            }
            return Json(new { success = false, message = "E-posta veya ÅŸifre hatalÄ±." });
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
                // 1. NVI DOÄRULAMASI
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
                    ModelState.AddModelError("TcIdentityNo", "MERNÄ°S (NVI) doÄŸrulamasÄ± baÅŸarÄ±sÄ±z oldu. GirdiÄŸiniz bilgileri kontrol ediniz.");
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
                    PhoneNumberConfirmed = false, // Gecikmeli doÄŸrulama
                    EmailConfirmed = false, // Gecikmeli doÄŸrulama
                    ReferralCode = "GMK-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(), // Kendisinin referans kodu
                    RegisteredCampaignId = model.CampaignId
                };

                // 2. REFERANS KODU (ref) Ä°LE GELMÄ°ÅSE
                if (!string.IsNullOrEmpty(model.ReferralCode))
                {
                    var referrer = await _userManager.Users.FirstOrDefaultAsync(u => u.ReferralCode == model.ReferralCode);
                    if (referrer != null)
                    {
                        user.ReferredByUserId = referrer.Id;
                    }
                }

                // 3. GÃ–LGE KULLANICISI (Davet/Token) Ä°LE GELMÄ°ÅSE
                if (!string.IsNullOrEmpty(model.ShadowToken))
                {
                    // Token Ã§Ã¶zme mantÄ±ÄŸÄ± burada (Ã–rn: veritabanÄ±nda PendingInvites gibi bir tabloda aranÄ±r veya token JWT'dir)
                    // Åimdilik token geÃ§erli varsayÄ±yoruz ve rastgele bir adminin altÄ±na atÄ±yoruz (Demo amaÃ§lÄ±)
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

                    // 4. KURUMSAL VEYA USTA Ä°SE EK TABLOLARA KAYIT
                    if (user.UserType == UserType.Corporate)
                    {
                        var corp = new CorporateProfile
                        {
                            ApplicationUserId = user.Id,
                            CompanyName = model.CompanyName ?? $"{model.FirstName} Åirketi",
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

                    // Ãœcretsiz BaÅŸlangÄ±Ã§ AboneliÄŸi Ekle (KullanÄ±cÄ± Tipi Bireysel/YatÄ±rÄ±mcÄ±/Oturan ise)
                    if (user.UserType == UserType.Individual || user.UserType == UserType.PropertyOwner)
                    {
                        var freePackage = await _context.SubscriptionPackages.FirstOrDefaultAsync(p => p.Period == PackagePeriod.Free);
                        if (freePackage == null)
                        {
                            freePackage = new SubscriptionPackage
                            {
                                Name = "Dijital Evim - Ãœcretsiz BaÅŸlangÄ±Ã§",
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
                            EndDate = DateTime.UtcNow.AddDays(30), // 30 GÃ¼nlÃ¼k Deneme SÃ¼rÃ¼mÃ¼
                            IsActive = true,
                            PricePaid = 0,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.UserSubscriptions.Add(sub);
                        await _context.SaveChangesAsync();
                    }

                    // GiriÅŸ Yap
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    // AkÄ±llÄ± Sihirbaz (Wizard) YÃ¶nlendirmesi
                    if (user.UserType == UserType.Individual || user.UserType == UserType.PropertyOwner)
                    {
                        // Dijital Evim kurulum sihirbazÄ±na git
                        return RedirectToAction("SetupWizard", "DigitalHome");
                    }
                    else if (user.UserType == UserType.ServiceProvider)
                    {
                        // Usta profil kurulum sihirbazÄ±na git
                        return RedirectToAction("SetupWizard", "Usta");
                    }
                    else if (user.UserType == UserType.Corporate)
                    {
                        return RedirectToAction("SetupWizard", "Corporate");
                    }

                    // VarsayÄ±lan
                    return RedirectToAction("PropertyOwner", "Dashboard");
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
                    
                    // Referans hesaplamalarÄ±nÄ± yap
                    if (!string.IsNullOrEmpty(user.ReferredByUserId))
                    {
                        var referrer = await _userManager.FindByIdAsync(user.ReferredByUserId);
                        if (referrer != null)
                        {
                            await _walletService.AddGiftCreditAsync(referrer.Id, 25m, 30, $"Referans Ã–dÃ¼lÃ¼ (Yeni KullanÄ±cÄ±: {user.FirstName})");
                            await _walletService.AddGiftCreditAsync(user.Id, 50m, 30, "HoÅŸgeldin Hediyesi");
                        }
                    }
                    else 
                    {
                        await _walletService.AddGiftCreditAsync(user.Id, 50m, 30, "HoÅŸgeldin Hediyesi");
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Dashboard");
                }
            }

            ModelState.AddModelError(string.Empty, "GeÃ§ersiz doÄŸrulama kodu.");
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
                return NotFound($"GeÃ§ersiz kullanÄ±cÄ± ID'si: '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
                            if (result.Succeeded)
                {

                TempData["SuccessMessage"] = "E-posta adresiniz baÅŸarÄ±yla doÄŸrulandÄ±. Åimdi giriÅŸ yapabilirsiniz!";
                return RedirectToAction("Login", "Account");
            }

            TempData["ErrorMessage"] = "E-posta doÄŸrulama iÅŸlemi baÅŸarÄ±sÄ±z oldu.";
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

            // Test ortamÄ± iÃ§in kodu her zaman 123456 yapÄ±yoruz. GerÃ§ekte Random kullanÄ±lmalÄ±.
            var code = "123456"; // new Random().Next(100000, 999999).ToString();
            TempData["GeneratedDeviceCode"] = code;

            if (provider == "Email")
            {
                await _emailSender.SendEmailAsync(user.Email, "GMK360 - Yeni Cihaz DoÄŸrulama Kodu", $"GÃ¼venlik kodunuz: {code}");
                TempData["VerificationMessage"] = "DoÄŸrulama kodu e-posta adresinize gÃ¶nderildi.";
            }
            else if (provider == "SMS")
            {
                await _smsService.SmsGonderAsync(user.PhoneNumber, $"GMK360 cihaz doÄŸrulama kodunuz: {code}");
                TempData["VerificationMessage"] = "DoÄŸrulama kodu SMS ile telefonunuza gÃ¶nderildi.";
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

                // GiriÅŸ yap
                await _signInManager.SignInAsync(user, isPersistent: rememberMe);

                // CihazÄ± gÃ¼venilir olarak iÅŸaretle (1 yÄ±llÄ±k Ã§erez)
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

                // "Bu sen miydin?" E-postasÄ± GÃ¶nder (Arka planda)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailSender.SendEmailAsync(user.Email, "GMK360 - Yeni Cihaz GiriÅŸi", 
                            "HesabÄ±nÄ±za yeni bir cihazdan giriÅŸ yapÄ±ldÄ±. EÄŸer bu siz deÄŸilseniz hemen ÅŸifrenizi deÄŸiÅŸtirin.");
                    }
                    catch { }
                });

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "GirdiÄŸiniz kod hatalÄ±.");
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

            // VKN ModÃ¼l 10 DoÄŸrulamasÄ±
            if (!ValidateVkn(model.TaxNumber))
            {
                ModelState.AddModelError("TaxNumber", "GirdiÄŸiniz Vergi Kimlik NumarasÄ± (VKN) geÃ§ersizdir.");
                return View(model);
            }

            // NVI (KPSPublic) TCKN DoÄŸrulamasÄ± (Yetkili KiÅŸi Ä°Ã§in)
            var isNviValid = await _nviValidationService.ValidateTcIdentityAsync(
                model.TcIdentityNo, 
                model.FirstName, 
                model.LastName, 
                model.BirthYear
            );

            if (!isNviValid)
            {
                ModelState.AddModelError("TcIdentityNo", "NVI (Kimlik) doÄŸrulamasÄ± baÅŸarÄ±sÄ±z oldu. LÃ¼tfen bilgilerinizi kontrol ediniz.");
                return View(model);
            }

            // Subdomain unique validation
            var existingAgency = await _context.Agencies.FirstOrDefaultAsync(a => a.Subdomain == model.Subdomain);
            if (existingAgency != null)
            {
                ModelState.AddModelError("Subdomain", "Bu alt alan adÄ± (subdomain) zaten kullanÄ±lÄ±yor. LÃ¼tfen baÅŸka bir tane seÃ§iniz.");
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
                FirstName = model.FirstName, // Yetkili adÄ±
                LastName = model.LastName, // Yetkili soyadÄ±
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
                
                // Ana Ã‡atÄ± Kurumsal Profilini OluÅŸtur
                var corporateProfile = new CorporateProfile
                {
                    ApplicationUserId = user.Id,
                    CompanyName = model.CompanyName,
                    TaxOffice = model.TaxOffice ?? "Belirtilmedi",
                    TaxNumber = model.TaxNumber
                };
                _context.CorporateProfiles.Add(corporateProfile);

                await _context.SaveChangesAsync();
                
                // Telefon doÄŸrulamasÄ± (OTP) iÃ§in OTP oluÅŸtur
                var otp = new Random().Next(100000, 999999).ToString();
                TempData["SmsOtpCode"] = otp; // GerÃ§ek senaryoda veritabanÄ± veya Redis'te tutulur
                TempData["UserId"] = user.Id;

                string smsMsg = $"GMK360 Kurumsal doÄŸrulama kodunuz: {otp}. Kimseyle paylaÅŸmayÄ±nÄ±z.";
                await _smsService.SmsGonderAsync(user.PhoneNumber, smsMsg);

                // KayÄ±t tamam, OTP doÄŸrulama ekranÄ±na yÃ¶nlendir.
                return RedirectToAction("VerifyPhone");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("~/Views/Home/KurumsalFirmaKatilim.cshtml", model);
        }

        // VKN ModÃ¼l 10 DoÄŸrulama AlgoritmasÄ±
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
                TempData["ErrorMessage"] = $"DÄ±ÅŸ saÄŸlayÄ±cÄ±dan hata: {remoteError}";
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
                // Yeni kayÄ±t
                var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
                if (email != null)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        var firstName = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.GivenName) ?? "Google";
                        var lastName = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Surname) ?? "KullanÄ±cÄ±sÄ±";
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
                                // Onboarding kontrolÃƒÂ¼
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
                                // Onboarding kontrolÃƒÂ¼
                                if (!user.IsEDevletVerified || !user.PhoneNumberConfirmed)
                                {
                                    return RedirectToAction("Onboarding", "Account");
                                }
                                return RedirectToLocal(returnUrl);
                    }
                }
                TempData["ErrorMessage"] = "Google giriÅŸi baÅŸarÄ±sÄ±z oldu.";
                return RedirectToAction(nameof(Login));
            }
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Onboarding()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (await _userManager.IsInRoleAsync(user, "SuperAdmin") || await _userManager.IsInRoleAsync(user, "Admin")) { return RedirectToAction("Index", "Home", new { area = "GMK360" }); }

            // EÄŸer zaten onboarding tamamlandÄ±ysa
            if (user.PhoneNumberConfirmed && user.IsEDevletVerified)
            {
                return RedirectToAction("PropertyOwner", "Dashboard");
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

            // For testing purposes, fill in missing required fields with dummy data
            if (string.IsNullOrEmpty(model.FirstName)) model.FirstName = "Test";
            if (string.IsNullOrEmpty(model.LastName)) model.LastName = "User";
            if (string.IsNullOrEmpty(model.TcIdentityNo)) model.TcIdentityNo = "11111111111";
            if (model.BirthYear == 0) model.BirthYear = 1990;
            if (string.IsNullOrEmpty(model.PhoneNumber)) {
                var phoneVal = Request.Form["Phone"].ToString();
                model.PhoneNumber = !string.IsNullOrEmpty(phoneVal) ? phoneVal : "05555555555";
            }
            if (string.IsNullOrEmpty(model.SelectedUserType)) model.SelectedUserType = "Corporate";

            ModelState.Clear();
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // var isNviValid = true; // Test pass


            // TODO: SMS OTP Entegrasyonu (Åimdilik doÄŸrudan onaylÄ±yoruz)
            user.PhoneNumber = model.PhoneNumber;
            user.PhoneNumberConfirmed = true; // SMS kodu onaylanÄ±nca true yapÄ±lmalÄ± normalde

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.TcIdentityNo = model.TcIdentityNo;
            user.BirthYear = model.BirthYear;
            user.IsEDevletVerified = true;

            // SeÃ§ilen KullanÄ±cÄ± Tipini Ata
            if (Enum.TryParse<GMK360.Core.Entities.Identity.UserType>(model.SelectedUserType, out var userType))
            {
                user.UserType = userType;
            }

            if (user.IsShadowAccount)
            {
                user.IsShadowAccount = false;
            }

            await _userManager.UpdateAsync(user);

            // GiriÅŸ Ã§erezini tazele
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















