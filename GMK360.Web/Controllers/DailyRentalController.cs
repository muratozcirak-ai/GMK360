using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using GMK360.Web.Models;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class DailyRentalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DailyRentalController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        // 1. Yönetim Masası & Tesis Genel Bakışı
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var isSuperAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

            // Kullanıcının veya işletmenin günlük kiralık tesisini bul
            var query = _context.Buildings
                .Include(b => b.Units)
                .AsQueryable();

            Building? facility = null;

            if (!string.IsNullOrEmpty(userId))
            {
                facility = await query.FirstOrDefaultAsync(b => 
                    (b.ManagerUserId == userId || b.CreatedByUserId == userId) && 
                    (b.TechnicalFeatures == "DailyRental" || (b.Description != null && b.Description.Contains("Günlük Kiralık"))));
            }

            // Eğer özel tesis bulunamadıysa ve kullanıcı Admin ise, sistemdeki herhangi bir DailyRental tesisine bak
            if (facility == null && isSuperAdmin)
            {
                facility = await query.FirstOrDefaultAsync(b => 
                    b.TechnicalFeatures == "DailyRental" || (b.Description != null && b.Description.Contains("Günlük Kiralık")));
            }

            // Tesis henüz hiç kurulmamışsa: Sıfır mock verili onboarding karşılama sun
            if (facility == null)
            {
                var emptyModel = new DailyRentalWorkspaceViewModel
                {
                    HasFacility = false
                };
                return View(emptyModel);
            }

            // Tesis varsa: Gerçek odaları ve rezervasyonları yükle
            var units = facility.Units?
                .Where(u => u.IsAvailableForDailyRent || u.Category == "DailyRental")
                .OrderBy(u => u.DoorNumber)
                .ToList() ?? new List<BuildingUnit>();

            var unitIds = units.Select(u => u.Id).ToList();

            var reservations = await _context.Reservations
                .Include(r => r.TenantUser)
                .Include(r => r.Guests)
                .Include(r => r.Unit)
                .Where(r => unitIds.Contains(r.UnitId) && r.ReservationStatus != ReservationStatus.Cancelled)
                .OrderByDescending(r => r.StartDate)
                .Take(20)
                .ToListAsync();

            // KBS ayarı kontrolü
            var kbsSetting = await _context.KbsFacilitySettings
                .FirstOrDefaultAsync(k => k.ApplicationUserId == (facility.ManagerUserId ?? userId) && k.IsActive);

            var today = DateTime.Today;
            var todayCheckIns = reservations.Count(r => r.StartDate.Date == today);
            var todayCheckOuts = reservations.Count(r => r.EndDate.Date == today);

            var viewModel = new DailyRentalWorkspaceViewModel
            {
                HasFacility = true,
                FacilityId = facility.Id,
                FacilityName = facility.Name,
                FacilityType = facility.Description?.Contains("Pansiyon") == true ? "Butik Pansiyon" : 
                               facility.Description?.Contains("Villa") == true ? "Müstakil Villa" : "Apart Otel / Daire Grubu",
                Address = facility.Address ?? facility.StreetName ?? "Adres Tanımlı",
                CityName = facility.CityId.ToString(),
                DistrictName = facility.DistrictId.ToString(),
                KbsFacilityCode = kbsSetting?.EgmFacilityCode,
                TodayCheckInsCount = todayCheckIns,
                TodayCheckOutsCount = todayCheckOuts,
                Units = units.Select(u => {
                    var activeRes = reservations.FirstOrDefault(r => r.UnitId == u.Id && r.StartDate.Date <= today && r.EndDate.Date >= today);
                    var firstGuest = activeRes?.Guests?.FirstOrDefault();
                    var guestFullName = firstGuest != null ? $"{firstGuest.FirstName} {firstGuest.LastName}".Trim() : null;
                    return new DailyRentalUnitItemViewModel
                    {
                        Id = u.Id,
                        UnitName = string.IsNullOrWhiteSpace(u.DoorNumber) ? $"Birim #{u.Id}" : u.DoorNumber,
                        RoomLayout = u.RoomLayout ?? "Standart",
                        Capacity = u.GrossSquareMeters.HasValue && u.GrossSquareMeters.Value > 0 ? (int)u.GrossSquareMeters.Value : 2,
                        DailyRate = u.ListPrice ?? 1500m,
                        CleaningFee = 0,
                        IsOccupied = !u.IsEmpty || activeRes != null,
                        CurrentGuestName = guestFullName ?? activeRes?.TenantUser?.UserName,
                        CurrentCheckOutDate = activeRes?.EndDate,
                        CurrentReservationId = activeRes?.Id
                    };
                }).ToList(),
                Reservations = reservations.Select(r => {
                    var g = r.Guests?.FirstOrDefault();
                    var gName = g != null ? $"{g.FirstName} {g.LastName}".Trim() : (r.TenantUser?.UserName ?? "İsimsiz Misafir");
                    return new DailyRentalReservationItemViewModel
                    {
                        Id = r.Id,
                        UnitId = r.UnitId,
                        UnitName = r.Unit?.DoorNumber ?? $"Oda #{r.UnitId}",
                        GuestName = gName,
                        GuestPhone = r.TenantUser?.PhoneNumber ?? "-",
                        GuestIdNumber = g?.TCIdentityNumberOrPassport,
                        StartDate = r.StartDate,
                        EndDate = r.EndDate,
                        TotalPrice = r.TotalPrice,
                        Status = r.ReservationStatus.ToString(),
                        IsKbsReported = g != null && (!string.IsNullOrEmpty(g.TCIdentityNumberOrPassport) || g.IsReportedToKbs)
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        // 2. 0'dan Tesis & Oda Kurulum Sihirbazı (GET)
        [HttpGet]
        public IActionResult SetupWizard()
        {
            var model = new DailyRentalSetupWizardViewModel
            {
                FacilityName = "",
                FacilityType = "Apart Otel",
                CityName = "Muğla",
                DistrictName = "Bodrum",
                NeighborhoodName = "Yalıkavak",
                Units = new List<SetupUnitInputModel>
                {
                    new SetupUnitInputModel { Name = "Daire 1 (1+1)", RoomLayout = "1+1", Capacity = 3, DailyPrice = 2000, CleaningFee = 500 },
                    new SetupUnitInputModel { Name = "Daire 2 (2+1 Deniz Manzara)", RoomLayout = "2+1", Capacity = 5, DailyPrice = 3200, CleaningFee = 600 }
                }
            };

            return View(model);
        }

        // 3. 0'dan Tesis & Oda Kurulum Sihirbazı (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetupWizard(DailyRentalSetupWizardViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.FacilityName))
            {
                ModelState.AddModelError("FacilityName", "Lütfen bir tesis veya yer adı giriniz.");
                return View(model);
            }

            var userId = GetUserId();

            // İlk Şehir / İlçe Referanslarını Al
            var firstCity = await _context.Cities.FirstOrDefaultAsync();
            var firstDistrict = await _context.Districts.FirstOrDefaultAsync();

            int cityId = firstCity?.Id ?? 34;
            int districtId = firstDistrict?.Id ?? 1;

            // 1. Yeni Tesis / Bina Kaydı
            var building = new Building
            {
                Name = model.FacilityName.Trim(),
                TechnicalFeatures = "DailyRental",
                Description = $"Günlük Kiralık & Konaklama Tesisi ({model.FacilityType}) - Giriş: {model.CheckInTime}, Çıkış: {model.CheckOutTime}",
                Address = string.IsNullOrWhiteSpace(model.Address) ? $"{model.NeighborhoodName}, {model.DistrictName}/{model.CityName}" : model.Address,
                CityId = cityId,
                DistrictId = districtId,
                NeighborhoodId = 1,
                StreetName = model.NeighborhoodName,
                ManagerUserId = userId,
                CreatedByUserId = userId,
                CreatedAt = DateTime.Now,
                TotalUnits = model.Units != null && model.Units.Any() ? model.Units.Count : 1,
                HasBlock = false,
                TotalFloors = 2
            };

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            // 2. Odaları / Daireleri Ekle
            if (model.Units != null && model.Units.Any())
            {
                int floor = 1;
                foreach (var unitInput in model.Units.Where(u => !string.IsNullOrWhiteSpace(u.Name)))
                {
                    var unit = new BuildingUnit
                    {
                        BuildingId = building.Id,
                        DoorNumber = unitInput.Name.Trim(),
                        RoomLayout = unitInput.RoomLayout ?? "1+1",
                        FloorLevel = floor++,
                        GrossSquareMeters = unitInput.Capacity, // Kapasite saklamak için
                        ListPrice = unitInput.DailyPrice > 0 ? unitInput.DailyPrice : 1500m,
                        IsAvailableForDailyRent = true,
                        Category = "DailyRental",
                        IsEmpty = true, // İlk kurulumda temiz & müsait
                        FacadeDirection = "Güneş Alır"
                    };
                    _context.BuildingUnits.Add(unit);
                }
            }
            else
            {
                // Varsayılan tekil birim
                var defaultUnit = new BuildingUnit
                {
                    BuildingId = building.Id,
                    DoorNumber = "Daire 1 (1+1)",
                    RoomLayout = "1+1",
                    GrossSquareMeters = 3,
                    ListPrice = 2000m,
                    IsAvailableForDailyRent = true,
                    Category = "DailyRental",
                    IsEmpty = true
                };
                _context.BuildingUnits.Add(defaultUnit);
            }

            // 3. KBS Tesis Kodu girildiyse kaydet
            if (!string.IsNullOrWhiteSpace(model.KbsFacilityCode))
            {
                var kbs = new KbsFacilitySettings
                {
                    ApplicationUserId = userId,
                    EgmFacilityCode = model.KbsFacilityCode.Trim(),
                    EgmPasswordEncrypted = "KBS_CONFIGURED",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                _context.KbsFacilitySettings.Add(kbs);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Tebrikler! '{building.Name}' tesisiniz ve birimleriniz sıfırdan başarıyla oluşturuldu. Artık doğrudan rezervasyon alabilir ve yönetebilirsiniz.";
            return RedirectToAction(nameof(Index));
        }

        // 4. Hızlı Rezervasyon Kaydı Alma (Modal / AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickAddReservation(QuickReservationInputModel model)
        {
            var unit = await _context.BuildingUnits.FindAsync(model.UnitId);
            if (unit == null)
            {
                TempData["ErrorMessage"] = "Seçilen oda veya daire bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var userId = GetUserId();

            // Rezervasyon oluştur
            var reservation = new Reservation
            {
                UnitId = model.UnitId,
                TenantUserId = userId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                GuestCount = model.GuestCount > 0 ? model.GuestCount : 2,
                TotalPrice = model.TotalPrice > 0 ? model.TotalPrice : (unit.ListPrice ?? 1500m),
                ReservationStatus = model.PaymentStatus == "Tamamlandi" ? ReservationStatus.Confirmed : ReservationStatus.Pending,
                CreatedAt = DateTime.Now
            };

            // Misafir kimlik bilgisi
            var names = (model.GuestName ?? "Misafir").Trim().Split(' ', 2);
            var guest = new GuestIdentity
            {
                FirstName = names[0],
                LastName = names.Length > 1 ? names[1] : "",
                TCIdentityNumberOrPassport = model.GuestIdNumber?.Trim() ?? "",
                DateOfBirth = DateTime.Today.AddYears(-30),
                IsReportedToKbs = !string.IsNullOrEmpty(model.GuestIdNumber),
                CreatedAt = DateTime.Now
            };

            reservation.Guests.Add(guest);
            _context.Reservations.Add(reservation);

            // Odayı dolu yap
            unit.IsEmpty = false;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{unit.DoorNumber} için {model.GuestName} adına {reservation.TotalPrice:N0} ₺ tutarında rezervasyon kaydedildi.";
            return RedirectToAction(nameof(Index));
        }

        // 5. Mevcut Tesise Yeni Oda / Daire Ekleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSingleUnit(int buildingId, string unitName, string roomLayout, int capacity, decimal dailyPrice)
        {
            var building = await _context.Buildings.FindAsync(buildingId);
            if (building == null) return NotFound();

            var newUnit = new BuildingUnit
            {
                BuildingId = buildingId,
                DoorNumber = string.IsNullOrWhiteSpace(unitName) ? "Yeni Birim" : unitName.Trim(),
                RoomLayout = roomLayout ?? "1+1",
                GrossSquareMeters = capacity > 0 ? capacity : 2,
                ListPrice = dailyPrice > 0 ? dailyPrice : 1500m,
                IsAvailableForDailyRent = true,
                Category = "DailyRental",
                IsEmpty = true
            };

            _context.BuildingUnits.Add(newUnit);
            building.TotalUnits += 1;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{newUnit.DoorNumber}' başarıyla tesisinize eklendi.";
            return RedirectToAction(nameof(Index));
        }

        // 6. Oda / Daire Durumunu Değiştir (Müsait <-> Dolu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUnitStatus(int unitId)
        {
            var unit = await _context.BuildingUnits.FindAsync(unitId);
            if (unit == null) return NotFound();

            unit.IsEmpty = !unit.IsEmpty;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{unit.DoorNumber} durumu '{(unit.IsEmpty ? "Müsait / Temiz" : "Dolu / Temizlik Bekliyor")}' olarak güncellendi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
