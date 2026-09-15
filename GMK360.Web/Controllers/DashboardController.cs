using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Entities.Identity; // UserType enum için
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;

using GMK360.Web.ViewModels;

using GMK360.Web.Filters;
namespace GMK360.Web.Controllers
{
    [Authorize]
    [RequireOnboarding] public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DashboardController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Uygulama Merkezi (App Hub)
        public async Task<IActionResult> Hub()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.ActiveModules = user?.ActiveModules ?? "";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateModules([FromBody] string[] modules)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.ActiveModules = string.Join(",", modules);
                await _userManager.UpdateAsync(user);
                return Ok();
            }
            return BadRequest();
        }

        // Yönlendirici
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Hub));
        }

        // --- ROL BAZLI ÖZEL ANA SAYFALAR (Admin hepsine girebilir) ---

        [Authorize(Roles = "Individual,Admin")]
        public IActionResult Individual()
        {
            return View();
        }

        [Authorize(Roles = "PropertyOwner,Admin")]
        public async Task<IActionResult> PropertyOwner()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Sadece bu kullanıcıya ait ve "PrivateTracking" durumundaki (ilan olmayan) mülkleri çek
            var properties = await _context.Properties
                .Where(p => p.UserId == user.Id && p.State == GMK360.Core.Entities.ListingState.PrivateTracking && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Kullanıcıya ait bekleyen (ödenmemiş) finansal kayıtlar
            var pendingInvoicesCount = await _context.PropertyFinancialRecords
                .Include(f => f.Property)
                .Where(f => f.Property.UserId == user.Id && !f.IsCompleted)
                .CountAsync();

            // Aktif usta / tadilat talepleri
            var activeRenovationsCount = await _context.RenovationRequests
                .Where(r => r.UserId == user.Id && r.Status == GMK360.Core.Entities.RenovationStatus.Open)
                .CountAsync();

            // Örnek basit bir gelir gider hesabı (Kira giderleri vb.)
            // İleride daha detaylı finansal modüle bağlanabilir
            decimal totalIncome = 0;
            decimal totalExpense = 0;

            foreach (var prop in properties)
            {
                // Eğer kullanıcı "Ev Sahibi" ise kiradan gelir bekliyor varsayalım (Örnek)
                if (prop.ManagementRole == GMK360.Core.Entities.ManagementRole.Owner)
                {
                    totalIncome += prop.Price; // Temsili kira geliri
                }
                else if (prop.ManagementRole == GMK360.Core.Entities.ManagementRole.Tenant)
                {
                    totalExpense += prop.Price; // Temsili kira gideri
                }
            }

            var viewModel = new GMK360.Web.Models.Dashboard.PropertyOwnerDashboardViewModel
            {
                TotalProperties = properties.Count,
                TotalMonthlyIncome = totalIncome,
                TotalMonthlyExpense = totalExpense,
                PendingInvoices = pendingInvoicesCount,
                ActiveRenovationRequests = activeRenovationsCount,
                RecentProperties = properties.Take(5).ToList() // Son 5 mülk
            };

            return View(viewModel);
        }

        [Authorize(Roles = "InsaatFirmasi,Admin,Corporate")]
        public async Task<IActionResult> Construction()
        {
            var userId = _userManager.GetUserId(User);
            var consultant = await _context.AgencyConsultants.Include(c => c.Agency).FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);
            if (consultant == null) return View();

            var agency = consultant.Agency;
            ViewBag.Agency = agency;

            // Dynamic Counts
            var activeProjects = await _context.ConstructionProjects.CountAsync(p => p.AgencyId == agency.Id && p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Aktif_Santiye && !p.IsDeleted);
            var delayedPhases = await _context.ContractPhases.Include(p => p.Contract).CountAsync(p => p.Contract.AgencyId == agency.Id && !p.IsCompleted && p.TargetDate < DateTime.UtcNow);
            var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var totalPaymentsThisMonth = await _context.SubcontractorHakedisler
                .Include(p => p.Contract)
                .Where(p => p.Contract.AgencyId == agency.Id && p.IsApproved == true && p.HakedisDate >= firstDayOfMonth)
                .SumAsync(p => p.ClaimAmount - p.DeductionAmount);

            ViewBag.ActiveProjects = activeProjects;
            ViewBag.DelayedPhases = delayedPhases;
            ViewBag.TotalPaymentsThisMonth = totalPaymentsThisMonth;
            var teklifProjeler = await _context.ConstructionProjects
                .Where(p => p.AgencyId == agency.Id && p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();
            ViewBag.TeklifProjeler = teklifProjeler;
            
            // Yaklaşan Ödemeler (Gelecek 15 Gün içinde vadesi dolan Çek ve Kredi Kartı ödemeleri)
            var in15Days = DateTime.UtcNow.AddDays(15);
            var upcomingPayments = await _context.Set<GMK360.Core.Entities.Finance.SupplierPayment>()
                .Include(p => p.SupplierCurrentAccount)
                    .ThenInclude(c => c.PhonebookContact)
                .Where(p => p.SupplierCurrentAccount.AgencyId == agency.Id 
                       && p.DueDate.HasValue 
                       && p.DueDate.Value <= in15Days 
                       && p.Status == GMK360.Core.Entities.Finance.PaymentStatus.Pending)
                .OrderBy(p => p.DueDate)
                .Take(5)
                .ToListAsync();
            
            ViewBag.UpcomingPayments = upcomingPayments;

            return View();
        }

        [Authorize(Roles = "Corporate,Admin,InsaatFirmasi,Emlakci")]
        public IActionResult Corporate()
        {
            var subdomain = HttpContext.Items["Subdomain"] as string;
            ViewBag.Subdomain = subdomain;
            return View();
        }

        [Authorize(Roles = "Corporate,Admin")]
        public async Task<IActionResult> ThemeSettings()
        {
            var user = await _userManager.Users
                .Include(u => u.AgencyConsultants)
                .ThenInclude(ac => ac.Agency)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            var agency = user?.AgencyConsultants?.FirstOrDefault()?.Agency;
            if (agency == null)
            {
                return NotFound("Kullanıcıya ait bir emlak ofisi bulunamadı.");
            }

            var model = new ThemeSettingsViewModel
            {
                AgencyId = agency.Id,
                CustomDomain = agency.CustomDomain,
                Subdomain = agency.Subdomain,
                ThemePrimaryColor = agency.ThemePrimaryColor,
                ThemeSecondaryColor = agency.ThemeSecondaryColor,
                ThemeAccentColor = agency.ThemeAccentColor,
                FontFamily = agency.FontFamily,
                LogoUrl = agency.LogoUrl,
                FooterLogoUrl = agency.FooterLogoUrl,
                FaviconUrl = agency.FaviconUrl
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Corporate,Admin")]
        public async Task<IActionResult> ThemeSettings(ThemeSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.Users
                .Include(u => u.AgencyConsultants)
                .ThenInclude(ac => ac.Agency)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            var agency = user?.AgencyConsultants?.FirstOrDefault()?.Agency;
            if (agency == null || agency.Id != model.AgencyId)
            {
                return NotFound("Geçersiz emlak ofisi işlemi.");
            }

            agency.CustomDomain = model.CustomDomain;
            agency.Subdomain = model.Subdomain;
            agency.ThemePrimaryColor = model.ThemePrimaryColor;
            agency.ThemeSecondaryColor = model.ThemeSecondaryColor;
            agency.ThemeAccentColor = model.ThemeAccentColor;
            agency.FontFamily = model.FontFamily;
            agency.LogoUrl = model.LogoUrl;
            agency.FooterLogoUrl = model.FooterLogoUrl;
            agency.FaviconUrl = model.FaviconUrl;

            _context.Update(agency);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tema ayarları başarıyla güncellendi.";
            return RedirectToAction(nameof(ThemeSettings));
        }

        [Authorize(Roles = "Consultant,Admin")]
        public IActionResult Consultant()
        {
            return View();
        }

        [Authorize(Roles = "ServiceProvider,Admin")]
        public IActionResult ServiceProvider()
        {
            return View();
        }

        [Authorize(Roles = "CommercialRenter,Admin")]
        public IActionResult CommercialRenter()
        {
            return View();
        }

        [Authorize(Roles = "Supplier,Admin")]
        public IActionResult Supplier()
        {
            return View(); // Şimdilik basitçe döndürüyoruz
        }

        // --- ORTAK MODÜLLER (İleride taşınabilir veya burada kalabilir) ---

        public IActionResult Mesajlar() => View();

        [Authorize(Roles = "Corporate,Admin")]
        public IActionResult Ekibim() => View();

        [Authorize(Roles = "Corporate,Consultant,Admin")]
        public IActionResult Portfoyum() => View();

        [Authorize(Roles = "Corporate,Consultant,ServiceProvider,Admin")]
        public IActionResult Cuzdanim() => View();
        
        [Authorize(Roles = "ServiceProvider,Admin")]
        public IActionResult UstaProfilim() => View();

        [Authorize(Roles = "ServiceProvider,Admin")]
        public IActionResult PaketYonetimi() => View();

        [Authorize(Roles = "ServiceProvider,Admin")]
        public IActionResult UstaReferans() => View();

        [Authorize(Roles = "ServiceProvider,Supplier,CommercialRenter,Admin")]
        public IActionResult FinansCari() => View();

        [Authorize(Roles = "ServiceProvider,Supplier,Admin")]
        public IActionResult FaturaBilgilerim() => View();

        [Authorize(Roles = "InsaatFirmasi,Corporate,Admin")]
        public IActionResult Projelerim() => RedirectToAction("Index", "ConstructionProject");

        [Authorize(Roles = "InsaatFirmasi,Corporate,Admin")]
        public IActionResult SiteYonetimi() => View();
    }
}







