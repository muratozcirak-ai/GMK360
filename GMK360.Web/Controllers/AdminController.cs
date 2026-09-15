using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminController : Controller
    {
        private readonly GMK360.Data.Contexts.ApplicationDbContext _context;

        public AdminController(GMK360.Data.Contexts.ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Kumanda Merkezi (Süper Hub)
        public IActionResult Index()
        {
            return View();
        }

        // 2. Katalog ve Tanımlamalar (DefinitionsHub)
        // Mevcut DefinitionController ile entegre, buradan yönlendirilebilir veya UI burada birleştirilebilir.
        public IActionResult Definitions()
        {
            return RedirectToAction("Index", "Definition");
        }

        // 3. İlan ve Portföy Merkezi
        
        public IActionResult Users()
        {
            return RedirectToAction("Index", "AdminCRM");
        }

        public IActionResult Roles()
        {
            return View();
        }

        public IActionResult Packages()
        {
            var packages = _context.SubscriptionPackages.OrderByDescending(p => p.Id).ToList();
            return View(packages);
        }

        [HttpPost]
        public IActionResult CreatePackage(GMK360.Core.Entities.SubscriptionPackage package)
        {
            package.IsActive = true;
            _context.SubscriptionPackages.Add(package);
            _context.SaveChanges();
            return RedirectToAction(nameof(Packages));
        }

        [HttpPost]
        public IActionResult TogglePackage(int id)
        {
            var package = _context.SubscriptionPackages.Find(id);
            if (package != null)
            {
                package.IsActive = !package.IsActive;
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Packages));
        }

        public IActionResult ContactLeads()
        {
            var messages = _context.ContactMessages.OrderByDescending(m => m.SentAt).ToList();
            return View(messages);
        }

        public IActionResult Properties()
        {
            return View();
        }

        // 4. Finans ve Cüzdan Merkezi
        public IActionResult Finances()
        {
            return View();
        }

        // 5. Destek ve Müdahale Merkezi (Şifresiz Müdahale)
        public IActionResult SupportTickets()
        {
            return View();
        }

        public IActionResult LocationIntelligence()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> SeedDengeGrup(
            [FromServices] GMK360.Data.Contexts.ApplicationDbContext context,
            [FromServices] Microsoft.AspNetCore.Identity.UserManager<GMK360.Core.Entities.Identity.ApplicationUser> userManager,
            [FromServices] Microsoft.AspNetCore.Identity.RoleManager<GMK360.Core.Entities.Identity.ApplicationRole> roleManager)
        {
            var agency = context.Agencies.FirstOrDefault(a => a.Subdomain == "dengegrup" || a.Subdomain == "testfirma");
            if (agency == null)
            {
                agency = new GMK360.Core.Entities.Agency 
                { 
                    CompanyName = "Denge Grup İnşaat", 
                    Subdomain = "dengegrup",
                    ThemePrimaryColor = "#0056b3",
                    ThemeSecondaryColor = "#ffffff",
                    FontFamily = "Arial, sans-serif",
                    LogoUrl = "http://dengegrup.net/wp-content/uploads/2022/08/Denge-Grup-Logo-2.jpg",
                    Address = "Kadıköy, İstanbul",
                    TaxNumber = "1234567890",
                    WhatsAppNumber = "905551234567",
                    AuthCertificateNo = "3400001",
                    BannerUrl = "https://dengegrup.net/wp-content/uploads/2016/08/dummy-1.jpg",
                    IsActive = true
                };
                context.Agencies.Add(agency);
                await context.SaveChangesAsync();
            }
            
            // Create user Cengiz Bayraktar
            var email = "cengizfly1@hotmail.com";
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new GMK360.Core.Entities.Identity.ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = "Cengiz",
                    LastName = "Bayraktar",
                    UserType = GMK360.Core.Entities.Identity.UserType.Corporate,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, "Password123*");
                
                // Add role
                if (!await roleManager.RoleExistsAsync("InsaatFirmasi"))
                {
                    await roleManager.CreateAsync(new GMK360.Core.Entities.Identity.ApplicationRole { Name = "InsaatFirmasi" });
                }
                await userManager.AddToRoleAsync(user, "InsaatFirmasi");
            }

            // Her durumda kullanıcının firmaya bağlı olduğundan emin ol (Önceki yarıda kalan işlemleri onarmak için)
            var isLinked = context.Set<GMK360.Core.Entities.AgencyConsultant>().Any(ac => ac.UserId == user.Id && ac.AgencyId == agency.Id);
            if (!isLinked)
            {
                context.Set<GMK360.Core.Entities.AgencyConsultant>().Add(new GMK360.Core.Entities.AgencyConsultant
                {
                    AgencyId = agency.Id,
                    UserId = user.Id,
                    IsActive = true
                });
            }
            else
            {
                agency.LogoUrl = "http://dengegrup.net/wp-content/uploads/2022/08/Denge-Grup-Logo-2.jpg";
                agency.ThemePrimaryColor = "#0056b3";
                agency.FontFamily = "Arial, sans-serif";
            }
            
            var oldBlocks = context.AgencyWebBlocks.Where(b => b.AgencyId == agency.Id);
            context.AgencyWebBlocks.RemoveRange(oldBlocks);
            
            context.AgencyWebBlocks.Add(new GMK360.Core.Entities.AgencyWebBlock
            {
                AgencyId = agency.Id,
                BlockType = "HeroSlider",
                OrderIndex = 1,
                Title = "Denge Grup 25 Yılı Aşkın Güvenceyle",
                Subtitle = "Geçmişinden aldığı güç ile girişimcilik, yenilikçi ve dürüst ilkeler...",
                ContentHtml = "",
                ImageUrl = "https://dengegrup.net/wp-content/uploads/2023/01/WhatsApp-Image-2023-01-09-at-09.52.03.jpeg",
                TargetUrl = "#"
            });
            
            context.AgencyWebBlocks.Add(new GMK360.Core.Entities.AgencyWebBlock
            {
                AgencyId = agency.Id,
                BlockType = "FeatureBanner",
                OrderIndex = 2,
                Title = "Kaliteli Malzemeler, Güvenli Yapılar",
                Subtitle = "",
                ContentHtml = "<p>Kadıköy ve Bağdat Caddesi'nde onlarca tamamlanmış kentsel dönüşüm projesi.</p>",
                ImageUrl = "https://dengegrup.net/wp-content/uploads/2016/08/dummy-1.jpg",
                TargetUrl = "/AgencyStore/ProjectDetail/1"
            });
            
            // Projeleri ekle
            if (!context.Set<GMK360.Core.Entities.Construction.ConstructionProject>().Any(p => p.AgencyId == agency.Id))
            {
                context.Set<GMK360.Core.Entities.Construction.ConstructionProject>().Add(new GMK360.Core.Entities.Construction.ConstructionProject
                {
                    Name = "Denge Life Çamlıca",
                    Description = "Modern mimari ve lüks yaşamın yeni adresi.",
                    AgencyId = agency.Id,
                    Address = "Üsküdar, İstanbul",
                    StartDate = System.DateTime.UtcNow.AddMonths(1),
                    Status = GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif, // Yakında Başlayacak
                    CoverImageUrl = "https://dengegrup.net/wp-content/uploads/2016/08/dummy-1.jpg"
                });

                context.Set<GMK360.Core.Entities.Construction.ConstructionProject>().Add(new GMK360.Core.Entities.Construction.ConstructionProject
                {
                    Name = "Denge Towers Kadıköy",
                    Description = "Bağdat caddesine yürüme mesafesinde kentsel dönüşüm projesi.",
                    AgencyId = agency.Id,
                    Address = "Kadıköy, İstanbul",
                    StartDate = System.DateTime.UtcNow.AddMonths(-6),
                    Status = GMK360.Core.Entities.Construction.ProjectStatus.Aktif_Santiye, // Devam Eden
                    CoverImageUrl = "https://dengegrup.net/wp-content/uploads/2023/01/WhatsApp-Image-2023-01-09-at-09.52.03.jpeg"
                });
            }
            
            await context.SaveChangesAsync();
            return Content("Denge Grup site blocklari ve örnek projeleri basariyla eklendi. Test icin dengegrup.localhost:5248 adresine gidebilirsiniz.");
        }
    }
}




