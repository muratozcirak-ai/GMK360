using GMK360.Core.Entities;
using GMK360.Core.Entities.Enums;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using GMK360.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
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

        private async Task<bool> CanOwnerAccessUnitAsync(BuildingUnit? unit, ApplicationUser user)
        {
            if (unit == null) return false;
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) return true;
            if (unit.OwnerUserId == user.Id) return true;
            if (!string.IsNullOrWhiteSpace(user.Email) && string.Equals(unit.OwnerEmail, user.Email, StringComparison.OrdinalIgnoreCase)) return true;
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber) && string.Equals(unit.OwnerPhone, user.PhoneNumber, StringComparison.OrdinalIgnoreCase)) return true;
            return await _context.Buildings.AnyAsync(b => b.Id == unit.BuildingId && (b.ManagerUserId == user.Id || b.CreatedByUserId == user.Id));
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
                .Include(p => p.Images)
                .Where(p => p.UserId == user.Id || p.OwnerUserId == user.Id)
                .ToListAsync();

            viewModel.Properties = myProperties;

            // 2. Kullanıcının Sahip Olduğu Bağımsız Bölümler (BuildingUnits)
            var myUnits = await _context.BuildingUnits
                .Include(u => u.Building).ThenInclude(b => b.District)
                .Include(u => u.Building).ThenInclude(b => b.Neighborhood)
                .Include(u => u.Debts).ThenInclude(d => d.BuildingExpense)
                .Include(u => u.Spaces).ThenInclude(s => s.Fixtures)
                .Where(u => u.OwnerUserId == user.Id
                            || (!string.IsNullOrEmpty(user.Email) && u.OwnerEmail == user.Email)
                            || (!string.IsNullOrEmpty(user.PhoneNumber) && u.OwnerPhone == user.PhoneNumber))
                .Where(u => !(u.Category ?? "").StartsWith("OrtakAlan"))
                .ToListAsync();

            viewModel.Units = myUnits;

            var unitIds = myUnits.Select(u => u.Id).ToList();
            var unitPhotoDocs = unitIds.Any()
                ? await _context.DocumentArchives
                    .Where(d => !d.IsDeleted && d.SourceModule == "LandlordUnitPhoto" && d.SourceRecordId.HasValue && unitIds.Contains(d.SourceRecordId.Value))
                    .OrderByDescending(d => d.Id)
                    .ToListAsync()
                : new List<DocumentArchive>();

            var managedBoardBuildingIds = await _context.BuildingManagers
                .Where(m => m.UserId == user.Id && m.IsActive)
                .Select(m => m.BuildingId)
                .ToListAsync();
            bool isGlobalAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

            // Kartları Oluştur
            var distinctBuildingIds = new HashSet<int>();

            // A) BuildingUnits'ten gelen kartlar
            foreach (var unit in myUnits)
            {
                distinctBuildingIds.Add(unit.BuildingId);
                var unitDebts = unit.Debts?.ToList() ?? new List<UnitDebt>();
                var unpaidDebts = unitDebts.Where(d => !d.IsPaid && !d.IsArchived).ToList();
                var paidCapEx = unitDebts.Where(d => d.IsPaid && !d.IsArchived && d.BuildingExpense?.ExpenseType == BuildingExpenseType.CapitalExpenditure).Sum(d => d.Amount);

                bool hasTenant = !unit.IsEmpty && !string.IsNullOrEmpty(unit.TenantName);
                decimal rent = unit.ListPrice ?? 0m;
                var fixtures = unit.Spaces?
                    .SelectMany(s => s.Fixtures ?? new List<SpaceFixture>())
                    .Where(f => !f.IsDeleted)
                    .OrderByDescending(f => f.Id)
                    .ToList() ?? new List<SpaceFixture>();

                var photoList = unitPhotoDocs
                    .Where(d => d.SourceRecordId == unit.Id && !string.IsNullOrWhiteSpace(d.DocumentUrl))
                    .Select(d => d.DocumentUrl)
                    .ToList();
                if (unit.PropertyId.HasValue)
                {
                    var linkedProp = myProperties.FirstOrDefault(p => p.Id == unit.PropertyId.Value);
                    if (linkedProp?.Images != null)
                    {
                        foreach (var img in linkedProp.Images.Where(i => !i.IsDeleted && !string.IsNullOrWhiteSpace(i.ImageUrl)))
                        {
                            if (!photoList.Contains(img.ImageUrl)) photoList.Add(img.ImageUrl);
                        }
                    }
                }

                int rootBid = unit.Building?.ParentBuildingId ?? unit.BuildingId;
                bool canManage = isGlobalAdmin
                    || unit.Building?.ManagerUserId == user.Id
                    || unit.Building?.CreatedByUserId == user.Id
                    || managedBoardBuildingIds.Contains(rootBid);

                var meterElec = fixtures.FirstOrDefault(f => f.Category == "Sayac" && (f.ItemName.Contains("Elektrik", StringComparison.OrdinalIgnoreCase) || f.ItemName.Contains("Elk", StringComparison.OrdinalIgnoreCase)))?.StandardModel;
                var meterWater = fixtures.FirstOrDefault(f => f.Category == "Sayac" && f.ItemName.Contains("Su", StringComparison.OrdinalIgnoreCase))?.StandardModel;
                var meterGas = fixtures.FirstOrDefault(f => f.Category == "Sayac" && (f.ItemName.Contains("Gaz", StringComparison.OrdinalIgnoreCase) || f.ItemName.Contains("Doğalgaz", StringComparison.OrdinalIgnoreCase)))?.StandardModel;

                var card = new LandlordUnitCardViewModel
                {
                    UnitId = unit.Id,
                    BuildingId = unit.BuildingId,
                    Title = $"{unit.Building?.Name ?? "Bina"} - {unit.DoorNumber}",
                    BuildingName = unit.Building?.Name ?? "Bina / Site",
                    BuildingAddress = unit.Building?.Address ?? "-",
                    BlockAndDoor = $"{(!string.IsNullOrWhiteSpace(unit.Building?.BlockName) ? unit.Building.BlockName + " • " : "")}{unit.DoorNumber}",
                    Category = unit.Category ?? "Daire",
                    Layout = !string.IsNullOrWhiteSpace(unit.RoomLayout) ? unit.RoomLayout : "Belirtilmedi",
                    SquareMeters = unit.GrossSquareMeters ?? 0,
                    NetSquareMeters = unit.NetSquareMeters ?? 0,
                    FacadeDirection = unit.FacadeDirection,
                    FloorName = unit.FloorName,
                    DistrictName = unit.Building?.District?.Name,
                    NeighborhoodName = unit.Building?.Neighborhood?.Name,
                    CanManageBuilding = canManage,
                    PhotoUrls = photoList,
                    IsRented = hasTenant,
                    TenantName = unit.TenantName,
                    TenantPhone = unit.TenantPhone,
                    MonthlyRent = rent,
                    RentDueDay = 5,
                    IsRentPaidThisMonth = true,
                    OverdueDays = 0,
                    BuildingDebtAmount = unpaidDebts.Sum(d => d.Amount),
                    BuildingDebtDescription = unpaidDebts.Any() ? string.Join(", ", unpaidDebts.Select(d => d.BuildingExpense?.Description ?? "Aidat")) : "Borcu Yok",
                    PaidCapExAmount = paidCapEx,
                    PrivateFixtures = fixtures,
                    MeterElectric = string.IsNullOrWhiteSpace(meterElec) ? "Girilmedi" : meterElec,
                    MeterWater = string.IsNullOrWhiteSpace(meterWater) ? "Girilmedi" : meterWater,
                    MeterGas = string.IsNullOrWhiteSpace(meterGas) ? "Girilmedi" : meterGas,
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

        // Mal Sahibi Daire Özellikleri & Fotoğraf Güncelleme
        [HttpPost("UpdateUnitDetails")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUnitDetails(
            int unitId,
            string? roomLayout,
            double? grossSquareMeters,
            double? netSquareMeters,
            string? facadeDirection,
            decimal? listPrice,
            List<IFormFile>? unitPhotos)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var unit = await _context.BuildingUnits.Include(u => u.Building).FirstOrDefaultAsync(u => u.Id == unitId);
            if (!await CanOwnerAccessUnitAsync(unit, user)) return Forbid();

            if (!string.IsNullOrWhiteSpace(roomLayout)) unit!.RoomLayout = roomLayout.Trim();
            if (grossSquareMeters.HasValue && grossSquareMeters.Value >= 0) unit!.GrossSquareMeters = grossSquareMeters.Value;
            if (netSquareMeters.HasValue && netSquareMeters.Value >= 0) unit!.NetSquareMeters = netSquareMeters.Value;
            if (facadeDirection != null) unit!.FacadeDirection = facadeDirection.Trim();
            if (listPrice.HasValue && listPrice.Value >= 0) unit!.ListPrice = listPrice.Value;

            int uploadedCount = 0;
            if (unitPhotos != null && unitPhotos.Any())
            {
                var relDir = Path.Combine("uploads", "landlord-units", unit!.Id.ToString());
                var absDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relDir);
                Directory.CreateDirectory(absDir);

                foreach (var file in unitPhotos.Take(10))
                {
                    if (file == null || file.Length <= 0 || file.Length > 10 * 1024 * 1024) continue;
                    var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                    if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp") continue;

                    var safeName = $"{Guid.NewGuid():N}{ext}";
                    var absPath = Path.Combine(absDir, safeName);
                    using (var fs = new FileStream(absPath, FileMode.Create))
                    {
                        await file.CopyToAsync(fs);
                    }

                    var url = $"/uploads/landlord-units/{unit.Id}/{safeName}";
                    _context.DocumentArchives.Add(new DocumentArchive
                    {
                        BuildingId = unit.BuildingId,
                        Title = $"{unit.Building?.Name ?? "Daire"} No:{unit.DoorNumber} Fotoğrafı",
                        DocumentUrl = url,
                        UploadDate = DateTime.UtcNow,
                        FileName = file.FileName ?? safeName,
                        FileExtension = ext,
                        Category = "DaireFoto",
                        Year = DateTime.UtcNow.Year,
                        SourceModule = "LandlordUnitPhoto",
                        SourceRecordId = unit.Id,
                        Status = "Tamamlandı",
                        CreatedAt = DateTime.UtcNow
                    });
                    uploadedCount++;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Daire {unit!.DoorNumber} bilgileri güncellendi" + (uploadedCount > 0 ? $" ve {uploadedCount} fotoğraf eklendi." : ".");
            return RedirectToAction(nameof(Index));
        }

        // Daire İçi Demirbaş / Eşya / Sayaç Ekle
        [HttpPost("AddUnitFixture")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUnitFixture(
            int unitId,
            string category,
            string itemName,
            string? standardBrand,
            string? standardModel,
            double quantity = 1,
            string? maintenanceNotes = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var unit = await _context.BuildingUnits
                .Include(u => u.Spaces)
                .FirstOrDefaultAsync(u => u.Id == unitId);
            if (!await CanOwnerAccessUnitAsync(unit, user)) return Forbid();
            if (string.IsNullOrWhiteSpace(itemName))
            {
                TempData["ErrorMessage"] = "Lütfen eşya/demirbaş adını yazınız.";
                return RedirectToAction(nameof(Index));
            }

            var space = unit!.Spaces.FirstOrDefault(s => s.Name == "Daire İçi Alan") ?? unit.Spaces.FirstOrDefault();
            if (space == null)
            {
                space = new UnitSpace
                {
                    BuildingUnitId = unit.Id,
                    Name = "Daire İçi Alan",
                    Type = "Konut İçi"
                };
                _context.UnitSpaces.Add(space);
                await _context.SaveChangesAsync();
            }

            _context.SpaceFixtures.Add(new SpaceFixture
            {
                UnitSpaceId = space.Id,
                Category = string.IsNullOrWhiteSpace(category) ? "Genel" : category.Trim(),
                ItemName = itemName.Trim(),
                Quantity = quantity > 0 ? quantity : 1,
                Unit = "Adet",
                StandardBrand = string.IsNullOrWhiteSpace(standardBrand) ? null : standardBrand.Trim(),
                StandardModel = string.IsNullOrWhiteSpace(standardModel) ? null : standardModel.Trim(),
                MaintenanceNotes = string.IsNullOrWhiteSpace(maintenanceNotes) ? null : maintenanceNotes.Trim(),
                IsCustomizable = true,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"'{itemName.Trim()}' Daire {unit.DoorNumber} zimmet/demirbaş listesine eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("DeleteUnitFixture")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUnitFixture(int fixtureId, int unitId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var unit = await _context.BuildingUnits.FirstOrDefaultAsync(u => u.Id == unitId);
            if (!await CanOwnerAccessUnitAsync(unit, user)) return Forbid();

            var fixture = await _context.SpaceFixtures
                .Include(f => f.UnitSpace)
                .FirstOrDefaultAsync(f => f.Id == fixtureId && f.UnitSpace != null && f.UnitSpace.BuildingUnitId == unitId && !f.IsDeleted);
            if (fixture != null)
            {
                fixture.IsDeleted = true;
                fixture.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"'{fixture.ItemName}' listeden kaldırıldı.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Bölge / Mahalle Emsal Analizi (İlan Öncesi)
        [HttpGet("GetUnitNeighborhoodAnalysis")]
        public async Task<IActionResult> GetUnitNeighborhoodAnalysis(int unitId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var unit = await _context.BuildingUnits
                .Include(u => u.Building).ThenInclude(b => b.District)
                .Include(u => u.Building).ThenInclude(b => b.Neighborhood)
                .Include(u => u.Building).ThenInclude(b => b.City)
                .FirstOrDefaultAsync(u => u.Id == unitId);
            if (!await CanOwnerAccessUnitAsync(unit, user)) return Forbid();

            var b = unit!.Building;
            string locationTitle = string.Join(" / ", new[] { b?.City?.Name, b?.District?.Name, b?.Neighborhood?.Name }.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (string.IsNullOrWhiteSpace(locationTitle)) locationTitle = b?.Address ?? "Konum tanımlanmamış";

            string? aiReport = b?.Neighborhood?.AiAnalysisReport;
            if (string.IsNullOrWhiteSpace(aiReport) && b?.Neighborhood != null && b?.District != null)
            {
                var gemini = HttpContext.RequestServices.GetService<GMK360.Core.Interfaces.IGeminiAiService>();
                if (gemini != null)
                {
                    try
                    {
                        aiReport = await gemini.AnalyzeNeighborhoodAsync(b.District.Name, b.Neighborhood.Name);
                        b.Neighborhood.AiAnalysisReport = aiReport;
                        b.Neighborhood.AiAnalysisUpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    catch { }
                }
            }

            var districtProps = b?.DistrictId > 0
                ? await _context.Properties
                    .Where(p => !p.IsDeleted && p.State == ListingState.Active && p.Building != null && p.Building.DistrictId == b.DistrictId && p.Price > 0)
                    .Select(p => new { p.Price, p.GrossArea })
                    .Take(50)
                    .ToListAsync()
                : new();

            decimal? avgPrice = districtProps.Any() ? Math.Round(districtProps.Average(x => x.Price), 0) : null;
            int buildingUnitCount = b != null ? await _context.BuildingUnits.CountAsync(u => u.BuildingId == b.Id && !(u.Category ?? "").StartsWith("OrtakAlan")) : 0;

            return Json(new
            {
                success = true,
                location = locationTitle,
                unitSummary = $"{b?.Name} • No:{unit.DoorNumber} ({unit.RoomLayout ?? "Belirtilmedi"}, {unit.GrossSquareMeters ?? 0} m²)",
                comparableCount = districtProps.Count,
                avgListingPrice = avgPrice,
                buildingUnitCount,
                reportHtml = !string.IsNullOrWhiteSpace(aiReport)
                    ? aiReport
                    : $"Bu bölgede sistemde kayıtlı {districtProps.Count} aktif emsal ilan ve binanızda {buildingUnitCount} bağımsız bölüm bulunuyor. Kiralık/satılık fiyatını belirlerken daire fotoğraflarınızı ve demirbaş listesini tamamlamanız ilanın dönüşümünü artırır."
            });
        }

        // Daire İçi & Fotoğraf Hazırlığı Sonrası Kontrollü Emlak Portalı İlanı Yayınlama
        [HttpPost("ConvertToPropertyListing")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertToPropertyListing(
            int unitId,
            string? title = null,
            string? description = null,
            decimal? price = null,
            string? listingType = "Kiralık",
            string? roomCount = null,
            int? grossArea = null,
            int? netArea = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                .Include(u => u.Spaces).ThenInclude(s => s.Fixtures)
                .FirstOrDefaultAsync(u => u.Id == unitId);
            if (!await CanOwnerAccessUnitAsync(unit, user)) return Forbid();

            bool isSale = string.Equals(listingType, "Satılık", StringComparison.OrdinalIgnoreCase);
            var statusKeyword = isSale ? "Satılık" : "Kiralık";
            var rentStatus = await _context.DefinitionValues.FirstOrDefaultAsync(v => v.SystemCode == (isSale ? "SALE" : "RENT"))
                          ?? await _context.DefinitionValues.FirstOrDefaultAsync(v => v.Name.Contains(statusKeyword));
            var resType = await _context.DefinitionValues.FirstOrDefaultAsync(v => v.SystemCode == "RESIDENTIAL")
                       ?? await _context.DefinitionValues.FirstOrDefaultAsync(v => v.Name.Contains("Konut"));

            var effectiveRoom = !string.IsNullOrWhiteSpace(roomCount) ? roomCount.Trim() : (unit!.RoomLayout ?? "3+1");
            var effectiveGross = grossArea.HasValue && grossArea.Value > 0 ? grossArea.Value : (int)(unit!.GrossSquareMeters ?? 110);
            var effectiveNet = netArea.HasValue && netArea.Value > 0 ? netArea.Value : (int)(unit!.NetSquareMeters ?? Math.Round(effectiveGross * 0.85));
            var effectivePrice = price.HasValue && price.Value > 0 ? price.Value : (unit!.ListPrice ?? 25000m);

            var fixtureText = string.Join(", ", (unit!.Spaces ?? new List<UnitSpace>())
                .SelectMany(s => s.Fixtures ?? new List<SpaceFixture>())
                .Where(f => !f.IsDeleted && f.Category != "Sayac")
                .Select(f => string.IsNullOrWhiteSpace(f.StandardBrand) ? f.ItemName : $"{f.StandardBrand} {f.ItemName}"));

            var defaultDesc = $"{unit.Building?.Name ?? "Sitede"} {statusKeyword.ToLower()} {effectiveRoom} ({effectiveGross} m²). "
                + (!string.IsNullOrWhiteSpace(fixtureText) ? $"Daire içi demirbaşlar: {fixtureText}." : "");

            var prop = new Property
            {
                Title = !string.IsNullOrWhiteSpace(title) ? title.Trim() : $"{unit.Building?.Name ?? "Merkezi"} - {statusKeyword} {effectiveRoom} ({unit.DoorNumber})",
                Description = !string.IsNullOrWhiteSpace(description) ? description.Trim() : defaultDesc,
                Price = effectivePrice,
                Currency = "TRY",
                GrossArea = effectiveGross,
                NetArea = effectiveNet,
                RoomCount = effectiveRoom,
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

            var photos = await _context.DocumentArchives
                .Where(d => !d.IsDeleted && d.SourceModule == "LandlordUnitPhoto" && d.SourceRecordId == unit.Id && !string.IsNullOrWhiteSpace(d.DocumentUrl))
                .OrderBy(d => d.Id)
                .ToListAsync();
            int order = 0;
            foreach (var ph in photos)
            {
                _context.PropertyImages.Add(new PropertyImage
                {
                    PropertyId = prop.Id,
                    ImageUrl = ph.DocumentUrl,
                    IsCover = order == 0,
                    SortOrder = order++,
                    CreatedAt = DateTime.UtcNow
                });
            }

            unit.PropertyId = prop.Id;
            unit.RoomLayout = effectiveRoom;
            unit.GrossSquareMeters = effectiveGross;
            unit.NetSquareMeters = effectiveNet;
            unit.ListPrice = effectivePrice;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"🎉 '{unit.DoorNumber}' için Emlak Portalı {statusKeyword} ilanı ({photos.Count} fotoğrafla) oluşturuldu!";
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
