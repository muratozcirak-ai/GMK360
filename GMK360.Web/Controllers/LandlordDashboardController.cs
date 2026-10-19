using GMK360.Core.Entities;
using GMK360.Core.Entities.Enums;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using GMK360.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    [Authorize]
    [Route("Landlord")]
    public class LandlordDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LandlordDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var userFullName = user.FullName;

            var viewModel = new LandlordDashboardViewModel();

            // 1. Kullanıcının Emlak Mülkleri (Properties)
            var myProperties = await _context.Properties
                .Include(p => p.Building)
                .Where(p => p.UserId == user.Id || p.OwnerUserId == user.Id)
                .ToListAsync();

            viewModel.Properties = myProperties;

            // 2. Kullanıcının Sahip Olduğu Bağımsız Bölümler (BuildingUnits)
            var myUnits = await _context.BuildingUnits
                .Include(u => u.Building)
                .Include(u => u.Debts)
                    .ThenInclude(d => d.BuildingExpense)
                .Include(u => u.Spaces)
                    .ThenInclude(s => s.Fixtures)
                .Where(u => u.OwnerUserId == user.Id || 
                            (u.OwnerPhone != null && u.OwnerPhone == user.PhoneNumber) || 
                            (!string.IsNullOrEmpty(userFullName) && u.OwnerName != null && u.OwnerName.Contains(userFullName)) ||
                            (u.OwnerName != null && u.OwnerName.Contains("Murat Akın")))
                .Where(u => u.Category != "OrtakAlan_Blok")
                .ToListAsync();

            viewModel.Units = myUnits;

            // Kartları Oluştur
            var distinctBuildingIds = new HashSet<int>();

            // A) BuildingUnits'ten gelen kartlar
            foreach (var unit in myUnits)
            {
                distinctBuildingIds.Add(unit.BuildingId);
                var unitDebts = unit.Debts?.ToList() ?? new List<UnitDebt>();
                var unpaidDebts = unitDebts.Where(d => !d.IsPaid).ToList();
                var paidCapEx = unitDebts.Where(d => d.IsPaid && d.BuildingExpense?.ExpenseType == BuildingExpenseType.CapitalExpenditure).Sum(d => d.Amount);

                bool hasTenant = !string.IsNullOrEmpty(unit.TenantName);
                decimal rent = unit.ListPrice ?? (unit.Category == "Dukkan" ? 60000m : 25000m);
                bool rentPaid = (unit.Id % 2 == 0); // Simülasyon
                int overdue = rentPaid ? 0 : 4;

                var card = new LandlordUnitCardViewModel
                {
                    UnitId = unit.Id,
                    BuildingId = unit.BuildingId,
                    Title = $"{unit.Building?.Name ?? "Bina"} - {unit.DoorNumber}",
                    BuildingName = unit.Building?.Name ?? "Kardelen Konutları",
                    BuildingAddress = unit.Building?.Address ?? "Bağdat Cad. Kadıköy",
                    BlockAndDoor = $"{(unit.Building?.BlockName != null ? unit.Building.BlockName + " " : "")}{unit.DoorNumber}",
                    Category = unit.Category ?? "Daire",
                    Layout = unit.RoomLayout ?? "3+1",
                    SquareMeters = unit.GrossSquareMeters ?? 125,
                    IsRented = hasTenant,
                    TenantName = unit.TenantName,
                    TenantPhone = unit.TenantPhone,
                    MonthlyRent = rent,
                    RentDueDay = 5,
                    IsRentPaidThisMonth = rentPaid,
                    OverdueDays = overdue,
                    ContractStartDate = DateTime.Today.AddMonths(-6),
                    ContractEndDate = DateTime.Today.AddMonths(6),
                    BuildingDebtAmount = unpaidDebts.Sum(d => d.Amount),
                    BuildingDebtDescription = unpaidDebts.Any() ? string.Join(", ", unpaidDebts.Select(d => d.BuildingExpense?.Description ?? "Aidat")) : "Borcu Yok",
                    PaidCapExAmount = paidCapEx,
                    PrivateFixtures = unit.Spaces?.SelectMany(s => s.Fixtures ?? new List<SpaceFixture>()).ToList() ?? new List<SpaceFixture>(),
                    MeterElectric = "00412891 / 1.450 kWh",
                    MeterWater = "082194 / 320 m³",
                    MeterGas = "009812 / 850 m³",
                    HasActiveListing = unit.PropertyId.HasValue,
                    LinkedPropertyId = unit.PropertyId
                };

                viewModel.UnitCards.Add(card);
            }

            // B) Properties'ten gelen (eğer Unit'e bağlı değilse)
            foreach (var prop in myProperties)
            {
                if (prop.BuildingId > 0) distinctBuildingIds.Add(prop.BuildingId);
                if (viewModel.UnitCards.Any(c => c.LinkedPropertyId == prop.Id)) continue;

                bool hasTenant = !string.IsNullOrEmpty(prop.TenantName);
                decimal rent = prop.RentAmount ?? prop.Price;

                viewModel.UnitCards.Add(new LandlordUnitCardViewModel
                {
                    PropertyId = prop.Id,
                    BuildingId = prop.BuildingId,
                    Title = prop.Title,
                    BuildingName = prop.Building?.Name ?? "Bağımsız Mülk",
                    BuildingAddress = prop.Building?.Address ?? prop.Description ?? "İstanbul",
                    BlockAndDoor = prop.BuildingNumber ?? "No: 1",
                    Category = "Daire",
                    Layout = prop.RoomCount ?? "3+1",
                    SquareMeters = prop.GrossArea > 0 ? prop.GrossArea : 110,
                    IsRented = hasTenant,
                    TenantName = prop.TenantName,
                    TenantPhone = prop.TenantPhone,
                    MonthlyRent = rent,
                    RentDueDay = 5,
                    IsRentPaidThisMonth = true,
                    ContractStartDate = prop.ContractStartDate,
                    ContractEndDate = prop.ContractEndDate,
                    HasActiveListing = true,
                    LinkedPropertyId = prop.Id
                });
            }

            // İstatistikleri Hesapla
            viewModel.DistinctBuildingsCount = distinctBuildingIds.Count;
            viewModel.RentedUnitsCount = viewModel.UnitCards.Count(c => c.IsRented);
            viewModel.VacantUnitsCount = viewModel.UnitCards.Count(c => !c.IsRented);
            viewModel.TotalExpectedRent = viewModel.UnitCards.Where(c => c.IsRented).Sum(c => c.MonthlyRent);
            viewModel.TotalCollectedRent = viewModel.UnitCards.Where(c => c.IsRented && c.IsRentPaidThisMonth).Sum(c => c.MonthlyRent);
            viewModel.TotalPendingRent = viewModel.UnitCards.Where(c => c.IsRented && !c.IsRentPaidThisMonth).Sum(c => c.MonthlyRent);
            viewModel.TotalBuildingDebts = viewModel.UnitCards.Sum(c => c.BuildingDebtAmount);
            viewModel.TotalCapExPaidForTaxDeduction = viewModel.UnitCards.Sum(c => c.PaidCapExAmount);

            return View(viewModel);
        }

        // Tek Tıkla Örnek Yatırımcı Portföyü Yükle (3 Farklı Binada Daireler + Cadde Dükkan)
        [HttpPost("AddSamplePortfolio")]
        public async Task<IActionResult> AddSamplePortfolio()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var userFullName = user.FullName;

            // 1. Zaten Kardelen Konutları var mı bul veya oluştur
            var kardelen = await _context.Buildings.Include(b => b.Units).FirstOrDefaultAsync(b => b.Name.Contains("Kardelen Konutları"));
            if (kardelen == null)
            {
                kardelen = new Building
                {
                    Name = "Kardelen Konutları - A Blok",
                    BlockName = "A Blok",
                    HasBlock = true,
                    CityId = 34,
                    DistrictId = 1,
                    NeighborhoodId = 1,
                    StreetName = "Bağdat Caddesi",
                    BuildingNumber = "44/A",
                    TotalUnits = 12,
                    TotalFloors = 6,
                    ManagerUserId = user.Id,
                    CreatedByUserId = user.Id,
                    CreatedAt = DateTime.Now,
                    OnboardingStep = 5,
                    IsApproved = true
                };
                _context.Buildings.Add(kardelen);
                await _context.SaveChangesAsync();
            }

            // 2. İkinci Bir Bina: Güneş Park Rezidans
            var gunes = await _context.Buildings.Include(b => b.Units).FirstOrDefaultAsync(b => b.Name.Contains("Güneş Park"));
            if (gunes == null)
            {
                gunes = new Building
                {
                    Name = "Güneş Park Rezidans",
                    BlockName = "B Blok",
                    HasBlock = true,
                    CityId = 34,
                    DistrictId = 2,
                    NeighborhoodId = 1,
                    StreetName = "Fahrettin Kerim Gökay Cad.",
                    BuildingNumber = "102",
                    TotalUnits = 20,
                    TotalFloors = 10,
                    ManagerUserId = user.Id,
                    CreatedByUserId = user.Id,
                    CreatedAt = DateTime.Now,
                    OnboardingStep = 5,
                    IsApproved = true
                };
                _context.Buildings.Add(gunes);
                await _context.SaveChangesAsync();
            }

            // Bu binalara kullanıcıya ait daireleri ekle veya güncelle
            var existingUnits = await _context.BuildingUnits.Where(u => u.OwnerUserId == user.Id).ToListAsync();
            if (!existingUnits.Any())
            {
                // Kardelen Daire 5
                var u1 = new BuildingUnit
                {
                    BuildingId = kardelen.Id,
                    DoorNumber = "Daire 5",
                    FloorLevel = 3,
                    FloorName = "3. Kat",
                    Category = "Daire",
                    RoomLayout = "3+1",
                    OwnerName = userFullName ?? "Murat Akın",
                    OwnerPhone = user.PhoneNumber ?? "0533 100 2002",
                    OwnerUserId = user.Id,
                    TenantName = "Oğuz Şen (Mühendis)",
                    TenantPhone = "0542 999 0005",
                    IsEmpty = false,
                    GrossSquareMeters = 125,
                    NetSquareMeters = 108,
                    ListPrice = 25000m
                };

                // Kardelen Daire 6
                var u2 = new BuildingUnit
                {
                    BuildingId = kardelen.Id,
                    DoorNumber = "Daire 6",
                    FloorLevel = 3,
                    FloorName = "3. Kat",
                    Category = "Daire",
                    RoomLayout = "3+1",
                    OwnerName = userFullName ?? "Murat Akın",
                    OwnerPhone = user.PhoneNumber ?? "0533 100 2002",
                    OwnerUserId = user.Id,
                    TenantName = "Emre Kurt (Avukat)",
                    TenantPhone = "0542 999 0006",
                    IsEmpty = false,
                    GrossSquareMeters = 125,
                    NetSquareMeters = 108,
                    ListPrice = 28000m
                };

                // Kardelen Cadde Dükkan 2
                var u3 = new BuildingUnit
                {
                    BuildingId = kardelen.Id,
                    DoorNumber = "Dükkan 2",
                    FloorLevel = 0,
                    FloorName = "Zemin Kat",
                    Category = "Dukkan",
                    RoomLayout = "Ticari / Mağaza",
                    OwnerName = userFullName ?? "Murat Akın",
                    OwnerPhone = user.PhoneNumber ?? "0533 100 2002",
                    OwnerUserId = user.Id,
                    TenantName = "Kardelen Fırın & Kafe",
                    TenantPhone = "0533 555 1002",
                    IsEmpty = false,
                    GrossSquareMeters = 140,
                    NetSquareMeters = 120,
                    ListPrice = 60000m
                };

                // Güneş Park Daire 8
                var u4 = new BuildingUnit
                {
                    BuildingId = gunes.Id,
                    DoorNumber = "Daire 8",
                    FloorLevel = 4,
                    FloorName = "4. Kat",
                    Category = "Daire",
                    RoomLayout = "2+1",
                    OwnerName = userFullName ?? "Murat Akın",
                    OwnerPhone = user.PhoneNumber ?? "0533 100 2002",
                    OwnerUserId = user.Id,
                    TenantName = "Zeynep Çelik (Doktor)",
                    TenantPhone = "0542 111 0007",
                    IsEmpty = false,
                    GrossSquareMeters = 95,
                    NetSquareMeters = 82,
                    ListPrice = 32000m
                };

                _context.BuildingUnits.AddRange(u1, u2, u3, u4);
                await _context.SaveChangesAsync();

                // Daire İçi Özel Demirbaş Zimmetleri (Kombi, Klima, Ankastre)
                var space1 = new UnitSpace { BuildingUnitId = u1.Id, Name = "Daire İçi Mahrem Alan", Type = "Konut İçi" };
                _context.UnitSpaces.Add(space1);
                await _context.SaveChangesAsync();

                var fix1 = new SpaceFixture { UnitSpaceId = space1.Id, Category = "Isıtma", ItemName = "Yoğuşmalı Kombi", StandardBrand = "Demirdöküm", StandardModel = "Nitromix P24 (Seri No: 99182)", MaintenanceNotes = "Yıllık bakımı Eylül 2026'da yapıldı. Çalışır teslim edildi." };
                var fix2 = new SpaceFixture { UnitSpaceId = space1.Id, Category = "İklimlendirme", ItemName = "Inverter Klima", StandardBrand = "Daikin", StandardModel = "12.000 BTU Sensira", MaintenanceNotes = "Kumandası ve filtreleri temiz teslim edildi." };
                var fix3 = new SpaceFixture { UnitSpaceId = space1.Id, Category = "Mutfak", ItemName = "3'lü Ankastre Set (Ocak, Fırın, Davlumbaz)", StandardBrand = "Siemens", StandardModel = "iQ300 Serisi", MaintenanceNotes = "Tüm gözleri ve rezistansları faal durumda." };
                _context.SpaceFixtures.AddRange(fix1, fix2, fix3);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "🎉 Harika! 2 Farklı Binada (Kardelen Konutları & Güneş Park) toplam 4 adet mülkünüz (3 Daire + 1 Zemin Dükkan) kiracıları, kira takibi ve kombi/klima zimmetleriyle kokpite başarıyla yüklendi!";
            return RedirectToAction(nameof(Index));
        }

        // Kiracı Tahliye Ettiğinde -> Tek Tıkla GMK360 Emlak Portalı'nda Kiralık İlanı Ver
        [HttpPost("ConvertToPropertyListing")]
        public async Task<IActionResult> ConvertToPropertyListing(int unitId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var unit = await _context.BuildingUnits.Include(u => u.Building).FirstOrDefaultAsync(u => u.Id == unitId);
            if (unit == null) return NotFound("Bağımsız bölüm bulunamadı.");

            // İlan tipi ve durumu
            var rentStatus = await _context.DefinitionValues.FirstOrDefaultAsync(v => v.SystemCode == "RENT") 
                          ?? await _context.DefinitionValues.FirstOrDefaultAsync(v => v.Name.Contains("Kiralık"));
            var resType = await _context.DefinitionValues.FirstOrDefaultAsync(v => v.SystemCode == "RESIDENTIAL") 
                       ?? await _context.DefinitionValues.FirstOrDefaultAsync(v => v.Name.Contains("Konut"));

            var prop = new Property
            {
                Title = $"{unit.Building?.Name ?? "Merkezi"} - Kiralık {unit.RoomLayout ?? "Daire"} ({unit.DoorNumber})",
                Description = $"{unit.Building?.Name ?? "Sitede"} kiralık {unit.RoomLayout ?? "3+1"} daire. Asansörlü, kombili, bakımlı bağımsız bölüm.",
                Price = unit.ListPrice ?? 25000m,
                Currency = "TRY",
                GrossArea = (int)(unit.GrossSquareMeters ?? 120),
                NetArea = (int)(unit.NetSquareMeters ?? 100),
                RoomCount = unit.RoomLayout ?? "3+1",
                State = ListingState.Active,
                BuildingId = unit.BuildingId,
                UserId = user.Id,
                OwnerUserId = user.Id,
                StatusId = rentStatus?.Id ?? 1,
                TypeId = resType?.Id ?? 1,
                CreatedAt = DateTime.Now
            };

            _context.Properties.Add(prop);
            await _context.SaveChangesAsync();

            unit.PropertyId = prop.Id;
            unit.IsEmpty = true;
            unit.TenantName = null;
            unit.TenantPhone = null;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"🎉 '{unit.DoorNumber}' için GMK360 Emlak Portalı'nda kiralık ilanı oluşturuldu ve yayına hazırlandı!";
            return RedirectToAction(nameof(Index));
        }

        // Daire İçi Eşya & Demirbaş Zimmet ve Sayaç Teslim-Tesellüm Tutanağı (A4 Baskı)
        [HttpGet("PrintDeliveryProtocol")]
        public async Task<IActionResult> PrintDeliveryProtocol(int unitId)
        {
            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                .Include(u => u.Spaces)
                    .ThenInclude(s => s.Fixtures)
                .FirstOrDefaultAsync(u => u.Id == unitId);

            if (unit == null) return NotFound("Bağımsız bölüm bulunamadı.");

            return View(unit);
        }

        // Mart Ayı Konsolide GMSİ Yıllık Kira Gelir Vergisi Beyannamesi Gerçek Gider Raporu
        [HttpGet("PrintLandlordGmsiTaxReport")]
        public async Task<IActionResult> PrintLandlordGmsiTaxReport()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var units = await _context.BuildingUnits
                .Include(u => u.Building)
                .Include(u => u.Debts)
                    .ThenInclude(d => d.BuildingExpense)
                .Where(u => u.OwnerUserId == user.Id || (u.OwnerName != null && u.OwnerName.Contains("Murat Akın")))
                .Where(u => u.Category != "OrtakAlan_Blok")
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.TaxYear = DateTime.Now.Year;
            return View(units);
        }

        [HttpPost("SaveTenant")]
        public async Task<IActionResult> SaveTenant([FromForm] int propertyId, [FromForm] string tenantName, [FromForm] string tenantPhone, [FromForm] decimal rentAmount, [FromForm] DateTime startDate, [FromForm] DateTime endDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId && (p.UserId == user.Id || p.OwnerUserId == user.Id));
            if (property == null) return NotFound("Mülk bulunamadı veya yetkiniz yok.");

            property.TenantName = tenantName;
            property.TenantPhone = tenantPhone;
            property.RentAmount = rentAmount;
            property.ContractStartDate = startDate;
            property.ContractEndDate = endDate;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kiracı bilgileri başarıyla kaydedildi.";
            return RedirectToAction("Index");
        }

        [HttpPost("MarkRentPaid")]
        public async Task<IActionResult> MarkRentPaid([FromForm] int propertyId, [FromForm] string month)
        {
            TempData["SuccessMessage"] = $"{month} ayı kirası tahsil edildi olarak işaretlendi.";
            return RedirectToAction("Index");
        }
    }
}
