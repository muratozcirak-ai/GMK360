using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class CustomerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IWebHostEnvironment _hostEnvironment;

        public CustomerDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // 1. Abonelikler
            var subscriptions = await _context.UserSubscriptions
                .Include(s => s.Package)
                .Where(s => s.UserId == user.Id && s.IsActive)
                .ToListAsync();

            // 2. Sahibi Olduğu veya Kiracı Olduğu Mülkler
            var propertyRelations = await _context.PropertyUsers
                .Include(pu => pu.Property)
                .Where(pu => pu.UserId == user.Id && pu.EndDate == null)
                .ToListAsync();

            var ownedProperties = propertyRelations.Where(p => p.RoleType == 1).ToList();
            var rentedProperties = propertyRelations.Where(p => p.RoleType == 2).ToList();

            // 3. Yönettiği Binalar (BuildingManager veya ManagementMember üzerinden)
            // Bu projedeki yapıya göre Building tablosundaki ManagerUserId ile eşleştiriyoruz
            var managedBuildings = await _context.Buildings
                .Where(b => b.ManagerUserId == user.Id)
                .ToListAsync();

            // 4. Gölge Kullanıcı (Shadow Account) uyarısı
            ViewBag.IsShadow = user.IsShadowAccount;
            ViewBag.WalletBalance = user.RealMoneyBalance;

            ViewBag.Subscriptions = subscriptions;
            ViewBag.OwnedProperties = ownedProperties;
            ViewBag.RentedProperties = rentedProperties;
            ViewBag.ManagedBuildings = managedBuildings;

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> AddProperty()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            
            // Sadece Şehirleri ViewBag ile gönderelim, gerisi AJAX ile dolacak
            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            ViewBag.PropertyTypes = await _context.DefinitionValues.Where(dv => dv.Category.SystemCode == "PROPERTY_TYPE").OrderBy(dv => dv.Order).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProperty([FromBody] GMK360.Web.ViewModels.AddPropertyViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            Building building;

            // Kullanıcı var olan bir binayı seçti mi?
            if (model.ExistingBuildingId.HasValue && model.ExistingBuildingId > 0)
            {
                building = await _context.Buildings.FindAsync(model.ExistingBuildingId.Value);
                if (building == null) return NotFound("Bina bulunamadı.");
            }
            else
            {
                // Yeni bina oluştur
                building = new Building
                {
                    Name = model.NewBuildingName,
                    CityId = model.CityId,
                    DistrictId = model.DistrictId,
                    NeighborhoodId = model.NeighborhoodId,
                    StreetId = model.StreetId,
                    StreetName = model.NewBuildingName,
                    ManagerUserId = user.Id, // Kuran kişi varsayılan yönetici
                    TotalUnits = 1
                };
                _context.Buildings.Add(building);
                await _context.SaveChangesAsync();
            }

            // Daireyi (Property) Oluştur
            var property = new Property
            {
                Title = $"{building.Name} - Daire {model.DoorNumber}",
                BuildingId = building.Id,
                RoomCount = model.RoomCount ?? "Bilinmiyor",
                StatusId = 1, // Satılık/Kiralık statüsü - Şimdilik dummy (Bireysel kullanım)
                TypeId = model.PropertyTypeId > 0 ? model.PropertyTypeId : 1,
                Price = 0,
                NetArea = model.NetArea ?? 0,
                GrossArea = model.GrossArea ?? 0
            };
            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            // Mülk ile Kullanıcıyı Eşleştir (RoleType: 1 = Owner, 2 = Tenant)
            var propertyUser = new PropertyUser
            {
                PropertyId = property.Id,
                UserId = user.Id,
                RoleType = model.RoleType // Formdan gelecek
            };
            _context.PropertyUsers.Add(propertyUser);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, propertyId = property.Id });
        }

        
        

        
        [HttpGet]
        public IActionResult SetupCorporateProfile()
        {
            return View(new GMK360.Web.Models.SetupCorporateProfileViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetupCorporateProfile(GMK360.Web.Models.SetupCorporateProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            
            string uploadedLogoUrl = "-"; // Default
            
            if (model.LogoFile != null && model.LogoFile.Length > 0)
            {
                // Upload dizinini ayarla
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "agencies", "logos");
                Directory.CreateDirectory(uploadsFolder);
                
                // Dosya adını benzersiz yap
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.LogoFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.LogoFile.CopyToAsync(fileStream);
                }
                
                uploadedLogoUrl = "/uploads/agencies/logos/" + uniqueFileName;
            }

            // 1. Yeni Ajans/Firma Oluştur
            var agency = new Agency
            {
                CompanyName = model.CompanyName,
                WhatsAppNumber = model.PhoneNumber,
                Address = "Girilmedi", // İleride formdan alınabilir
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                LogoUrl = uploadedLogoUrl, // DOSYADAN GELEN URL
                Subdomain = model.Subdomain,
                TaxNumber = model.TaxNumber, // Yeni eklenen Vergi Numarası
                AuthCertificateNo = "MUAF",
                
                // NOT NULL Kısıtlamasını aşmak için dummy değerler
                BannerUrl = "-",
                CustomDomain = "-",
                ThemePrimaryColor = "#f97316",
                ThemeSecondaryColor = "#1e3a8a",
                FontFamily = "Inter",
                FooterLogoUrl = "-",
                FaviconUrl = "-",
                ThemeAccentColor = "#ffffff"
            };


            _context.Agencies.Add(agency);
            await _context.SaveChangesAsync();

            // 2. Kullanıcıyı Firmaya Bağla
            var consultant = new AgencyConsultant
            {
                AgencyId = agency.Id,
                UserId = user.Id,
                IsActive = true,
                Role = AgencyRole.Owner,
                StartDate = DateTime.UtcNow
            };

            _context.AgencyConsultants.Add(consultant);
            await _context.SaveChangesAsync();

            // Firma kurulumu bittiğinde yönlendir
            return RedirectToAction("Construction", "Dashboard");
        }

        public async Task<IActionResult> Referrals()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // 1. Kullanıcının davet ettiği (ReferredByUserId == user.Id) kişiler
            var referredUsers = await _userManager.Users
                .Where(u => u.ReferredByUserId == user.Id)
                .Select(u => new 
                {
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    CreatedAt = u.Id // ID yerine tarih olmalı ama Identity tablolarında default CreatedAt yok, genelde claim veya custom field.
                                     // Basitleştirmek için sadece isim göstereceğiz.
                })
                .ToListAsync();

            // Kazanç hesaplama (Gerçek senaryoda EscrowTransaction üzerinden hesaplanır)
            // Şimdilik Cüzdan (RealMoneyBalance) veya ReferansLog tablosuna bakabiliriz.
            
            ViewBag.ReferredUsers = referredUsers;
            ViewBag.ReferralCode = user.ReferralCode ?? "Henüz Kod Oluşturulmadı";
            
            // Gelecekteki Potansiyel Kazanç Tahmini (Örn: Aktif aboneliklerin %25'i)
            // Gerçekçi bir hesaplama:
            var activeReferralSubscriptions = await _context.UserSubscriptions
                .Include(s => s.Package)
                .Where(s => s.User.ReferredByUserId == user.Id && s.IsActive && s.EndDate != null)
                .ToListAsync();

            decimal potentialEarnings = activeReferralSubscriptions.Sum(s => s.Package.Price * 0.25m); // %25 komisyon
            ViewBag.PotentialEarnings = potentialEarnings;

            return View(user);
        }
    }
}

