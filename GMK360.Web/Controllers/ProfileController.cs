using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using GMK360.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GMK360.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Dinamik Route: /usta/{slug}, /danisman/{slug}, /kurumsal/{slug}
        
        [Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            
            var currentUser = user;
            var pureOp = await GMK360.Web.Helpers.RoleLandingHelper.IsPureCorporateOperatorAsync(_userManager, _context, currentUser);
            ViewBag.ReturnPanelUrl = pureOp.IsPureCorporate ? pureOp.DirectPanelUrl : "/Dashboard/Hub";

            user = await _context.Users
                .Include(u => u.Subscriptions).ThenInclude(s => s.Package)
                .Include(u => u.Subscriptions).ThenInclude(s => s.DiscountPolicy)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id) ?? currentUser;

            ViewBag.LinkedUnits = await _context.BuildingUnits
                .Include(bu => bu.Building)
                .Where(bu => bu.OwnerUserId == user.Id || bu.TenantUserId == user.Id)
                .ToListAsync();
            ViewBag.SubscriptionSummary = GMK360.Web.Services.SaasSubscriptionService.BuildStatusSummary(user);

            return View(user);
        }

[HttpGet("{userTypeSlug:regex(^(usta|danisman|kurumsal)$)}/{profileSlug}")]
        public async Task<IActionResult> Detail(string userTypeSlug, string profileSlug)
        {
            // Kullanıcıyı Slug'dan bul (Büyük-küçük harf duyarsız arama)
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.ProfileSlug == profileSlug);

            if (user == null)
            {
                return NotFound("Profil bulunamadı.");
            }

            // Gelen URL type ile kullanıcının gerçek type'ı eşleşiyor mu kontrolü
            if ((userTypeSlug == "usta" && user.UserType != UserType.ServiceProvider) ||
                (userTypeSlug == "danisman" && user.UserType != UserType.Consultant) ||
                (userTypeSlug == "kurumsal" && user.UserType != UserType.Corporate))
            {
                return NotFound("Geçersiz profil url eşleşmesi.");
            }

            // Bireysel profil gizliliği (KVKK)
            if (user.UserType == UserType.Individual || user.UserType == UserType.PropertyOwner)
            {
                // TODO: Gelecekte yetkili mülk sahipleri vb. için açılabilir
                return Unauthorized("Bu kullanıcı tipi için açık profil sayfası bulunmamaktadır.");
            }

            var model = new PublicProfileViewModel
            {
                ProfileUserId = user.Id,
                DisplayName = !string.IsNullOrEmpty(user.DisplayName) ? user.DisplayName : $"{user.FirstName} {user.LastName}",
                UserType = user.UserType,
                ProfileImageUrl = user.ProfileImageUrl,
                Bio = "Platformumuza hoş geldiniz. Kullanıcı profil açıklaması eklenecektir.", // TODO: ApplicationUser'a Bio eklenebilir.
                IsVerifiedUser = user.IsEDevletVerified,
                ReliabilityScore = 100 // Varsayılan
            };

            if (user.UserType == UserType.ServiceProvider)
            {
                var provider = await _context.ServiceProviders
                    .Include(s => s.Services)
                        .ThenInclude(ss => ss.ServiceCategory)
                    .Include(s => s.Ratings)
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);

                if (provider != null)
                {
                    model.IsCorporateVerified = provider.IsCorporate; // veya IsVerified
                    model.IsEmergencyModeActive = provider.IsEmergencyModeActive;
                    model.ReliabilityScore = provider.ReliabilityScore;
                    model.ProviderServices = provider.Services.ToList();
                    model.AverageRating = provider.AverageRating;
                    model.TotalRatings = provider.TotalRatings;
                    model.BusinessName = provider.BusinessName;
                }
            }
            else if (user.UserType == UserType.Consultant || user.UserType == UserType.Corporate)
            {
                // Danışman / Kurumsal ilanlarını getir
                var activeListings = await _context.Properties
                    .Where(p => p.UserId == user.Id && p.State == ListingState.Active)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(12)
                    .ToListAsync();
                    
                model.ActiveListings = activeListings;
                
                var corp = await _context.CorporateProfiles.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
                if (corp != null)
                {
                    model.IsCorporateVerified = true; // Corp profile exists
                    model.BusinessName = corp.CompanyName;
                }
            }

            return View(model);
        }

        [HttpPost("Profile/AcceptMapConsent")]
        [Authorize]
        [IgnoreAntiforgeryToken] // Layout'tan ajax ile gelirken token uyuşmazlığı olmaması için
        public async Task<IActionResult> AcceptMapConsent()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.HasMapConsent = true;
                user.MapConsentDate = System.DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                return Ok();
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpPost("Profile/UpdateBasicInfo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBasicInfo(
            string firstName,
            string lastName,
            string? email,
            string? phoneNumber,
            string? tcIdentityNo = null,
            int? birthYear = null,
            string? address = null,
            int? addressCityId = null,
            int? addressDistrictId = null,
            int? addressNeighborhoodId = null,
            int? addressStreetId = null,
            string? addressStreetName = null,
            string? addressBuildingNo = null,
            string? addressApartmentNo = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!string.IsNullOrWhiteSpace(firstName)) user.FirstName = firstName.Trim();
            if (!string.IsNullOrWhiteSpace(lastName)) user.LastName = lastName.Trim();
            if (addressCityId.HasValue && addressCityId.Value > 0)
            {
                var resolved = await GMK360.Web.Helpers.AddressCaptureHelper.ResolveAndEnrichAsync(_context, new GMK360.Web.Helpers.StructuredAddressInput
                {
                    CityId = addressCityId,
                    DistrictId = addressDistrictId,
                    NeighborhoodId = addressNeighborhoodId,
                    StreetId = addressStreetId,
                    StreetName = addressStreetName,
                    BuildingNo = addressBuildingNo,
                    ApartmentNo = addressApartmentNo
                });
                if (!resolved.IsValid)
                {
                    TempData["ErrorMessage"] = "Lütfen adres için İl, İlçe, Mahalle, Sokak/Cadde ve Bina/Kapı No alanlarını eksiksiz seçiniz.";
                    return RedirectToAction(nameof(Index));
                }
                GMK360.Web.Helpers.AddressCaptureHelper.ApplyToUser(user, resolved);
            }
            else if (address != null)
            {
                user.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
            }

            if (string.IsNullOrWhiteSpace(user.TcIdentityNo) && !string.IsNullOrWhiteSpace(tcIdentityNo))
            {
                var cleanTc = GMK360.Web.Helpers.TcknHelper.Normalize(tcIdentityNo);
                if (!GMK360.Web.Helpers.TcknHelper.IsValidTcIdentity(cleanTc))
                {
                    TempData["ErrorMessage"] = "Geçersiz T.C. Kimlik Numarası (11 haneli algoritma doğrulamasından geçemedi).";
                    return RedirectToAction(nameof(Index));
                }
                if (await _context.Users.AnyAsync(u => u.Id != user.Id && u.TcIdentityNo == cleanTc))
                {
                    TempData["ErrorMessage"] = "Bu T.C. Kimlik Numarası başka bir GMK360 hesabında zaten kayıtlı.";
                    return RedirectToAction(nameof(Index));
                }
                user.TcIdentityNo = cleanTc;
            }
            if (!user.BirthYear.HasValue && birthYear.HasValue && birthYear.Value >= 1920 && birthYear.Value <= System.DateTime.UtcNow.Year)
            {
                user.BirthYear = birthYear.Value;
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var cleanEmail = email.Trim();
                if (!string.Equals(user.Email, cleanEmail, System.StringComparison.OrdinalIgnoreCase))
                {
                    var dup = await _userManager.FindByEmailAsync(cleanEmail);
                    if (dup != null && dup.Id != user.Id)
                    {
                        TempData["ErrorMessage"] = "Bu e-posta adresi başka bir kullanıcıda kayıtlı.";
                        return RedirectToAction(nameof(Index));
                    }
                    await _userManager.SetEmailAsync(user, cleanEmail);
                    await _userManager.SetUserNameAsync(user, cleanEmail);
                    user.EmailConfirmed = false;
                }
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber) && !string.Equals(user.PhoneNumber, phoneNumber.Trim(), System.StringComparison.Ordinal))
            {
                user.PhoneNumber = phoneNumber.Trim();
                user.PhoneNumberConfirmed = false;
                user.PhoneVerifiedAt = null;
            }

            await _userManager.UpdateAsync(user);
            var dispatchSvc = HttpContext.RequestServices.GetService(typeof(GMK360.Web.Services.FacilityTaskDispatchService)) as GMK360.Web.Services.FacilityTaskDispatchService;
            if (dispatchSvc != null) await dispatchSvc.SyncStaffProfileFromUserAsync(user);
            TempData["SuccessMessage"] = "Profil bilgileriniz güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost("Profile/RequestEmailVerify")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestEmailVerify()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            if (string.IsNullOrWhiteSpace(user.Email) || user.Email.EndsWith("@gmk360.local", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Önce 'Hesabım' sekmesinden geçerli kişisel e-posta adresinizi kaydedin.";
                return RedirectToAction(nameof(Index));
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var verifyUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);
            var emailSvc = HttpContext.RequestServices.GetService(typeof(GMK360.Core.Interfaces.IEmailService)) as GMK360.Core.Interfaces.IEmailService;
            if (emailSvc != null && !string.IsNullOrWhiteSpace(verifyUrl))
            {
                var body = $@"
<div style=""font-family:Arial,sans-serif;max-width:540px;margin:0 auto;padding:20px;border:1px solid #e2e8f0;border-radius:12px;"">
  <h2 style=""color:#1e3a8a;margin-top:0;"">GMK360 E-Posta Doğrulama</h2>
  <p>Merhaba <strong>{System.Net.WebUtility.HtmlEncode(user.DisplayName ?? user.Email)}</strong>,</p>
  <p>E-posta adresinizi doğrulamak için aşağıdaki butona tıklayın:</p>
  <p style=""margin:22px 0;""><a href=""{verifyUrl}"" style=""background:#1e3a8a;color:#fff;text-decoration:none;padding:12px 24px;border-radius:999px;font-weight:bold;display:inline-block;"">E-Postamı Doğrula</a></p>
  <p style=""font-size:12px;color:#64748b;"">Bağlantı: <a href=""{verifyUrl}"">{verifyUrl}</a></p>
</div>";
                var res = await emailSvc.SendEmailWithResultAsync(user.Email, "GMK360 E-Posta Doğrulama", body);
                TempData["EmailVerifyLink"] = verifyUrl;
                if (res.Success)
                {
                    TempData["SuccessMessage"] = $"Doğrulama e-postası '{user.Email}' adresine gönderildi.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"E-posta sunucusundan gönderilemedi ({res.Message}). Yedek doğrulama bağlantısı aşağıda hazırlandı.";
                }
                return RedirectToAction(nameof(Index));
            }
            TempData["EmailVerifyLink"] = verifyUrl;
            TempData["SuccessMessage"] = "E-posta doğrulama bağlantısı hazırlandı.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost("Profile/ConfirmPhoneNumberManual")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPhoneNumberManual(string phoneNumber)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                TempData["ErrorMessage"] = "Geçerli bir telefon numarası giriniz.";
                return RedirectToAction(nameof(Index));
            }
            user.PhoneNumber = phoneNumber.Trim();
            user.PhoneVerifiedAt = System.DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            TempData["SuccessMessage"] = "Telefon numaranız kaydedildi (SMS OTP entegrasyonu devreye girdiğinde otomatik doğrulama istenecektir).";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost("Profile/ChangeOwnPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeOwnPassword(string? currentPassword, string newPassword, string confirmPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Yeni şifre ve tekrarı uyuşmuyor.";
                return RedirectToAction(nameof(Index));
            }

            IdentityResult res;
            if (user.MustChangePassword || user.IsShadowAccount || string.IsNullOrEmpty(currentPassword))
            {
                await _userManager.RemovePasswordAsync(user);
                res = await _userManager.AddPasswordAsync(user, newPassword);
            }
            else
            {
                res = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            }

            if (!res.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(" ", res.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            user.MustChangePassword = false;
            user.IsShadowAccount = false;
            await _userManager.UpdateAsync(user);
            TempData["SuccessMessage"] = "Giriş şifreniz güvenli şekilde güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost("Profile/SetPersonalPin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPersonalPin(string personalPin)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var clean = (personalPin ?? "").Trim();
            if (clean.Length < 4 || clean.Length > 6 || !clean.All(char.IsDigit))
            {
                TempData["ErrorMessage"] = "Kişisel Onay & Mobil Giriş PIN'iniz 4 ila 6 haneli rakam olmalıdır.";
                return RedirectToAction(nameof(Index));
            }

            user.TransactionPinHash = GMK360.Web.Services.FacilityTaskDispatchService.HashQuickPin(clean);
            user.IsTransactionPinSet = true;
            await _userManager.UpdateAsync(user);
            var dispatchSvc = HttpContext.RequestServices.GetService(typeof(GMK360.Web.Services.FacilityTaskDispatchService)) as GMK360.Web.Services.FacilityTaskDispatchService;
            if (dispatchSvc != null) await dispatchSvc.SyncStaffProfileFromUserAsync(user);

            TempData["SuccessMessage"] = "🔐 Kişisel Onay ve PWA Giriş PIN'iniz güncellendi. Bu PIN yalnızca size özeldir ve yöneticiler dahil kimse tarafından görüntülenemez.";
            return RedirectToAction(nameof(Index));
        }

    }
}
