using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Entities.Identity;
using GMK360.Web.Models;

using GMK360.Web.Filters;
namespace GMK360.Web.Controllers
{
    [Authorize]
    [RequireOnboarding] public class DigitalHomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GMK360.Core.Interfaces.IWebhookService _webhookManager;
        private readonly GMK360.Core.Interfaces.IFinancialEngineService _financialEngine;

        public DigitalHomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, GMK360.Core.Interfaces.IWebhookService webhookManager, GMK360.Core.Interfaces.IFinancialEngineService financialEngine)
        {
            _context = context;
            _userManager = userManager;
            _webhookManager = webhookManager;
            _financialEngine = financialEngine;
        }

                [HttpGet]
        public async Task<IActionResult> GetNeighborhoodAnalysis(int propertyId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                                var property = await _context.Properties
                    .Include(p => p.Building)
                        .ThenInclude(b => b.Neighborhood)
                    .Include(p => p.Building)
                        .ThenInclude(b => b.District)
                    .FirstOrDefaultAsync(p => p.Id == propertyId && p.UserId == user.Id);

                if (property?.Building?.Neighborhood == null || property?.Building?.District == null)
                    return Json(new { success = false, message = "Lokasyon bilgisi eksik." });

                var neighborhood = property.Building.Neighborhood;
                var district = property.Building.District;

                // Eğer önbellekte rapor varsa ve 30 günden eskiyse (opsiyonel) veya hiç yoksa yeni rapor üret
                if (string.IsNullOrEmpty(neighborhood.AiAnalysisReport))
                {
                    var geminiService = HttpContext.RequestServices.GetService<GMK360.Core.Interfaces.IGeminiAiService>();
                    if (geminiService != null)
                    {
                        var report = await geminiService.AnalyzeNeighborhoodAsync(district.Name, neighborhood.Name);
                        
                        neighborhood.AiAnalysisReport = report;
                        neighborhood.AiAnalysisUpdatedAt = DateTime.UtcNow;

                        _context.Neighborhoods.Update(neighborhood);
                        await _context.SaveChangesAsync();
                    }
                }

                return Json(new { 
                    success = true, 
                    report = neighborhood.AiAnalysisReport,
                    updatedAt = neighborhood.AiAnalysisUpdatedAt?.ToString("dd.MM.yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Analiz üretilirken hata oluştu: " + ex.Message });
            }
        }

        // GET: Dijital Evim (Mülklerim)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var properties = await _context.Properties
                .Include(p => p.Building)
                    .ThenInclude(b => b.District)
                .Where(p => p.UserId == user.Id && p.State == ListingState.PrivateTracking)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Eğer kullanıcının henüz hiçbir dijital evi yoksa DOĞRUDAN SİHİRBAZA yönlendir
            if (!properties.Any())
            {
                return RedirectToAction("SetupWizard");
            }

            var propertyIds = properties.Select(p => p.Id).ToList();

            // Finansal Ajanda Verilerini Topla (Bu Ay İçin)
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // 1. Beklenen Kiralar (Aktif sözleşmeler üzerinden)
            var activeContracts = await _context.Set<TenancyContract>()
                .Where(c => propertyIds.Contains(c.PropertyId) && c.IsActive)
                .ToListAsync();

            decimal totalExpectedRent = activeContracts.Sum(c => c.MonthlyRentAmount);

            // 2. Bu Ayki Giderler
            var totalExpenses = await _context.Set<PropertyExpense>()
                .Where(e => propertyIds.Contains(e.PropertyId) && e.ExpenseDate >= startOfMonth && e.ExpenseDate <= endOfMonth)
                .SumAsync(e => e.Amount);

            // 3. Yaklaşan Etkinlikler (Önümüzdeki 30 gün)
            var upcomingEvents = new List<AgendaEvent>();
            var next30Days = now.AddDays(30);

            // 3.a Yaklaşan Kiralar
            foreach(var contract in activeContracts)
            {
                // Basit bir yaklaşım: Bu ayki ödeme günü gelmediyse veya önümüzdeki aya sarkıyorsa
                var paymentDate = new DateTime(now.Year, now.Month, contract.PaymentDay);
                if (paymentDate < now) paymentDate = paymentDate.AddMonths(1);

                if (paymentDate <= next30Days)
                {
                    upcomingEvents.Add(new AgendaEvent
                    {
                        Date = paymentDate,
                        Title = $"Kira Bekleniyor ({contract.TenantName})",
                        PropertyName = properties.First(p => p.Id == contract.PropertyId).Title ?? "İsimsiz Mülk",
                        Amount = contract.MonthlyRentAmount,
                        IsIncome = true,
                        IconClass = "ph-wallet",
                        ColorClass = "text-success",
                        StatusText = "Bekleniyor"
                    });
                }
            }

            // 3.b Yaklaşan Faturalar / Yükümlülükler (Örn: PropertyPayment tablosundan)
            var upcomingPayments = await _context.Set<PropertyPayment>()
                .Include(p => p.LiabilityType)
                .Where(p => propertyIds.Contains(p.PropertyId) && !p.IsPaid && p.DueDate >= now && p.DueDate <= next30Days)
                .ToListAsync();

            foreach(var pay in upcomingPayments)
            {
                upcomingEvents.Add(new AgendaEvent
                {
                    Date = pay.DueDate,
                    Title = pay.Description ?? pay.LiabilityType?.Name ?? "Ödeme",
                    PropertyName = properties.First(p => p.Id == pay.PropertyId).Title ?? "İsimsiz Mülk",
                    Amount = pay.Amount,
                    IsIncome = false,
                    IconClass = "ph-receipt",
                    ColorClass = "text-danger",
                    StatusText = "Ödenecek"
                });
            }

            // Tarihe göre sırala
            upcomingEvents = upcomingEvents.OrderBy(e => e.Date).ToList();

            var vm = new DigitalHomeDashboardViewModel
            {
                Properties = properties,
                FinancialAgenda = new FinancialAgendaViewModel
                {
                    TotalExpectedRentThisMonth = totalExpectedRent,
                    TotalExpensesThisMonth = totalExpenses,
                    UpcomingEvents = upcomingEvents
                }
            };

            return View(vm);
        }

        // GET: Dijital Evim Kurulum Sihirbazı (Adım 1: Adres)
        public async Task<IActionResult> SetupWizard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Sadece şehirleri getiriyoruz, ilçe vb. Ajax ile dolacak
            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();

            // Varsa yarım kalmış taslak mülk (draft) kaydını bulalım
            var draftProperty = await _context.Properties
                .Include(p => p.Building)
                .Where(p => p.UserId == user.Id && p.DraftStep > 0)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            return View(draftProperty);
        }

        [HttpGet]
        [Route("DigitalHome/SeedUtilities")]
        public async Task<IActionResult> SeedUtilities()
        {
            if(!_context.UtilityCompanies.Any())
            {
                _context.UtilityCompanies.AddRange(
                    new GMK360.Core.Entities.UtilityCompany { Name = "İSKİ", Type = "Su", IsActive = true, City = "İstanbul" },
                    new GMK360.Core.Entities.UtilityCompany { Name = "İGDAŞ", Type = "Doğalgaz", IsActive = true, City = "İstanbul" },
                    new GMK360.Core.Entities.UtilityCompany { Name = "BEDAŞ", Type = "Elektrik", IsActive = true, City = "İstanbul" },
                    new GMK360.Core.Entities.UtilityCompany { Name = "Türk Telekom", Type = "İnternet", IsActive = true },
                    new GMK360.Core.Entities.UtilityCompany { Name = "Turkcell Superonline", Type = "İnternet", IsActive = true },
                    new GMK360.Core.Entities.UtilityCompany { Name = "ASKİ", Type = "Su", IsActive = true, City = "Ankara" },
                    new GMK360.Core.Entities.UtilityCompany { Name = "Enerjisa", Type = "Elektrik", IsActive = true }
                );
                await _context.SaveChangesAsync();
                return Ok("Kurumlar başarıyla eklendi.");
            }
            return Ok("Kurumlar zaten mevcut.");
        }

        [HttpPost]
        public async Task<IActionResult> SaveWizardStep1(
            string city, string district, string neighborhood, string streetId, 
            string propertySubTypeId, string isComplex, string buildingId, string selectedComplexName, 
            string newBuildingName, string newBuildingNo, string complexName, string complexBlock, 
            string doorNumber, string lat, string lng, string normalBuildingBlockName)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                int parsedStreetId = 0;
                int.TryParse(streetId, out parsedStreetId);
                
                int parsedPropertySubTypeId = 0;
                int.TryParse(propertySubTypeId, out parsedPropertySubTypeId);
                
                bool parsedIsComplex = false;
                if (!string.IsNullOrEmpty(isComplex) && (isComplex.ToLower() == "true" || isComplex == "1" || isComplex.ToLower() == "on"))
                    parsedIsComplex = true;

                // Sadece şehir vs isimleri bul
                var dbCity = await _context.Cities.FirstOrDefaultAsync(c => c.Name == city);
                var dbDistrict = dbCity != null ? await _context.Districts.FirstOrDefaultAsync(d => d.Name == district && d.CityId == dbCity.Id) : null;
                var dbNeighborhood = dbDistrict != null ? await _context.Neighborhoods.FirstOrDefaultAsync(n => n.Name == neighborhood && n.DistrictId == dbDistrict.Id) : null;

                int cityId = dbCity?.Id ?? 0;
                int districtId = dbDistrict?.Id ?? 0;
                int neighborhoodId = dbNeighborhood?.Id ?? 0;
                
                if (cityId == 0 || districtId == 0 || neighborhoodId == 0)
                {
                    return BadRequest("Geçersiz konum bilgisi.");
                }

                // --- PRO PAKET KONTROLÜ (İkinci Bina Kısıtlaması) ---
                bool isPro = await _context.UserSubscriptions.AnyAsync(s => s.UserId == user.Id && s.IsActive && (s.EndDate == null || s.EndDate >= DateTime.UtcNow));
                var userBuildingIds = await _context.Properties.Where(p => p.UserId == user.Id && p.BuildingId != null).Select(p => p.BuildingId).Distinct().ToListAsync();

                if (!isPro && userBuildingIds.Count > 0)
                {
                    bool isSameBuilding = false;
                    
                    if (parsedIsComplex)
                    {
                        if (selectedComplexName != "NEW")
                        {
                            var existingComplexCheck = await _context.HousingComplexes.FirstOrDefaultAsync(hc => hc.Name == selectedComplexName && hc.StreetId == parsedStreetId);
                            if (existingComplexCheck != null)
                            {
                                var existingBlockCheck = await _context.Buildings.FirstOrDefaultAsync(b => b.HousingComplexId == existingComplexCheck.Id && b.BlockName == complexBlock);
                                if (existingBlockCheck != null && userBuildingIds.Contains(existingBlockCheck.Id))
                                {
                                    isSameBuilding = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (buildingId != "NEW")
                        {
                            if (int.TryParse(buildingId, out int bId) && userBuildingIds.Contains(bId))
                            {
                                isSameBuilding = true;
                            }
                        }
                    }

                    if (!isSameBuilding)
                    {
                        return Ok(new { success = false, requiresPro = true, message = "Ücretsiz planda sadece tek bir bina/lokasyonda mülk yönetebilirsiniz." });
                    }
                }
                // --- KONTROL BİTİŞİ ---

                Building buildingToUse = null;

                if (parsedIsComplex)
                {
                    string targetComplexName = selectedComplexName == "NEW" ? complexName : selectedComplexName;
                    
                    // 1. İlgili Siteyi Bul veya Yarat (HousingComplex)
                    var existingComplex = await _context.HousingComplexes
                        .FirstOrDefaultAsync(hc => hc.Name == targetComplexName && hc.StreetId == parsedStreetId);
                        
                    if (existingComplex == null)
                    {
                        existingComplex = new HousingComplex
                        {
                            Name = targetComplexName ?? "",
                            CityId = cityId,
                            DistrictId = districtId,
                            NeighborhoodId = neighborhoodId,
                            StreetId = parsedStreetId,
                            Address = "", // Sokak ve bina no ile hesaplanabilir
                            TotalBlocks = 1
                        };
                        _context.HousingComplexes.Add(existingComplex);
                        await _context.SaveChangesAsync();
                    }

                    // 2. İlgili Bloğu Bul veya Yarat (Building)
                    if (selectedComplexName == "NEW")
                    {
                        buildingToUse = new Building
                        {
                            Name = $"{targetComplexName} - {complexBlock} Blok",
                            HasBlock = true,
                            BlockName = complexBlock ?? "",
                            BuildingNumber = newBuildingNo ?? "",
                            StreetId = parsedStreetId,
                            HousingComplexId = existingComplex.Id,
                            CityId = cityId,
                            DistrictId = districtId,
                            NeighborhoodId = neighborhoodId,
                            ManagerUserId = user.Id,
                            CreatedAt = DateTime.UtcNow,
                            OnboardingStep = 1,
                            IsApproved = true
                        };
                        
                        if (!string.IsNullOrEmpty(lat) && double.TryParse(lat, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedLat))
                            buildingToUse.Latitude = parsedLat;
                        if (!string.IsNullOrEmpty(lng) && double.TryParse(lng, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedLng))
                            buildingToUse.Longitude = parsedLng;
                            
                        _context.Buildings.Add(buildingToUse);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        var existingBlock = await _context.Buildings.FirstOrDefaultAsync(b => b.HousingComplexId == existingComplex.Id && b.BlockName == complexBlock);
                        if (existingBlock != null)
                        {
                            buildingToUse = existingBlock;
                        }
                        else
                        {
                            var anyBlock = await _context.Buildings.FirstOrDefaultAsync(b => b.HousingComplexId == existingComplex.Id);
                            buildingToUse = new Building
                            {
                                Name = $"{targetComplexName} - {complexBlock} Blok",
                                HasBlock = true,
                                BlockName = complexBlock ?? "",
                                BuildingNumber = anyBlock?.BuildingNumber ?? "",
                                StreetId = parsedStreetId,
                                HousingComplexId = existingComplex.Id,
                                CityId = cityId,
                                DistrictId = districtId,
                                NeighborhoodId = neighborhoodId,
                                ManagerUserId = user.Id,
                                CreatedAt = DateTime.UtcNow,
                                OnboardingStep = 1,
                                IsApproved = true
                            };
                            
                            if (!string.IsNullOrEmpty(lat) && double.TryParse(lat, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedLat))
                                buildingToUse.Latitude = parsedLat;
                            if (!string.IsNullOrEmpty(lng) && double.TryParse(lng, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedLng))
                                buildingToUse.Longitude = parsedLng;
                                
                            _context.Buildings.Add(buildingToUse);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    if (buildingId == "NEW")
                    {
                        buildingToUse = new Building
                        {
                            Name = newBuildingName ?? "",
                            HasBlock = !string.IsNullOrWhiteSpace(normalBuildingBlockName),
                            BlockName = normalBuildingBlockName ?? "",
                            BuildingNumber = newBuildingNo ?? "",
                            StreetId = parsedStreetId,
                            StreetName = "",
                            CityId = cityId,
                            DistrictId = districtId,
                            NeighborhoodId = neighborhoodId,
                            ManagerUserId = user.Id,
                            CreatedAt = DateTime.UtcNow,
                            OnboardingStep = 1,
                            IsApproved = true
                        };
                        
                        if (!string.IsNullOrEmpty(lat) && double.TryParse(lat, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedLat))
                            buildingToUse.Latitude = parsedLat;
                        if (!string.IsNullOrEmpty(lng) && double.TryParse(lng, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedLng))
                            buildingToUse.Longitude = parsedLng;
                            
                        _context.Buildings.Add(buildingToUse);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        int bId = 0;
                        if (int.TryParse(buildingId, out bId))
                        {
                            buildingToUse = await _context.Buildings.FindAsync(bId);
                        }
                        if (buildingToUse == null)
                        {
                            buildingToUse = await _context.Buildings.FirstOrDefaultAsync(b => b.StreetId == parsedStreetId);
                        }
                    }
                }

                string title = buildingToUse.Name;
                if (buildingToUse.HasBlock && !string.IsNullOrEmpty(buildingToUse.BlockName))
                    title += $" ({buildingToUse.BlockName} Blok)";
                title += $" No: {doorNumber}";

                var defaultStatus = await _context.DefinitionValues.FirstOrDefaultAsync(d => d.CategoryId == 1) ?? await _context.DefinitionValues.FirstOrDefaultAsync();
                var defaultType = await _context.DefinitionValues.FirstOrDefaultAsync(d => d.CategoryId == 2) ?? await _context.DefinitionValues.FirstOrDefaultAsync();
                
                var prop = new Property
                {
                    Title = title ?? "",
                    Description = "",
                    ICalUrl = "",
                    VideoFilePath = "",
                    VideoUrl = "",
                    RoomCount = "",
                    BuildingAge = "",
                    DeedStatus = "",
                    BlockNumber = "",
                    ParcelNumber = "",
                    UserId = user.Id,
                    State = ListingState.PrivateTracking,
                    BuildingId = buildingToUse.Id,
                    DoorNumber = doorNumber ?? "1",
                    CreatedAt = DateTime.UtcNow,
                    SubTypeId = parsedPropertySubTypeId,
                    StatusId = defaultStatus?.Id ?? 1,
                    TypeId = defaultType?.Id ?? 1,
                    DraftStep = 1 // Adım 1 tamamlandı
                };

                _context.Properties.Add(prop);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, propertyId = prop.Id });
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message;
                if (ex.InnerException != null) errorMsg += " | Inner: " + ex.InnerException.Message;
                return BadRequest(errorMsg);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveWizardStep2(int propertyId, string title, int grossArea, int netArea, string roomCount, int bathroomCount, int floorNumber, string buildingAge, bool isFurnished, bool hasParking, bool hasElevator, bool hasShowerCabin)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var prop = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId && p.UserId == user.Id);
            if (prop == null) return NotFound();

            prop.Title = string.IsNullOrWhiteSpace(title) ? prop.Title : title;
            prop.GrossArea = grossArea;
            prop.NetArea = netArea;
            prop.RoomCount = roomCount;
            prop.BathroomCount = bathroomCount;
            prop.FloorNumber = floorNumber;
            prop.BuildingAge = buildingAge;
            prop.IsFurnished = isFurnished;
            
            // To be thorough, we can add a text note about parking/elevator if we don't map them to exact DefinitionValues right now.
            // But we will use the existing DB features if possible. We will store it in Description for AI context if needed.
            string extraFeatures = "";
            if (hasParking) extraFeatures += "Kapalı Otopark, ";
            if (hasElevator) extraFeatures += "Asansör, ";
            if (hasShowerCabin) extraFeatures += "Duşakabin, ";
            if (isFurnished) extraFeatures += "Eşyalı, ";

            if (!string.IsNullOrEmpty(extraFeatures))
            {
                prop.Description = $"Sihirbaz Ek Özellikleri: {extraFeatures.TrimEnd(',', ' ')}";
            }

            prop.DraftStep = 2; // Adım 2 tamamlandı
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> SaveWizardStep3(int propertyId, string userRole)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var prop = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId && p.UserId == user.Id);
            if (prop == null) return NotFound();

            ManagementRole mRole = ManagementRole.Owner;
            bool isResident = true;
            
            if (userRole == "Tenant")
            {
                mRole = ManagementRole.Tenant;
                isResident = true;
            }
            else if (userRole == "OwnerLiving")
            {
                mRole = ManagementRole.Owner;
                isResident = true;
            }
            else if (userRole == "OwnerInvestment")
            {
                mRole = ManagementRole.Owner;
                isResident = false;
            }

            prop.ManagementRole = mRole;
            prop.IsResident = isResident;
            prop.DraftStep = 3; // Adım 3 tamamlandı

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> SaveWizardStep4(
            int propertyId, decimal? duesAmount, decimal? duesDay, decimal? rentAmount, int? rentPaymentDay, 
            DateTime? contractStartDate, DateTime? contractEndDate, string? payeeFirstName, string? payeeLastName, bool useIyzico, 
            string? waterInstitution, string? waterSubscriberNo, 
            string? electricityInstitution, string? electricitySubscriberNo,
            string? tenantFirstName, string? tenantLastName, string? tenantPhone, string? tenantEmail, string? tenantIdentityNumber,
            bool isUtilitiesOnOwner, IFormFile? leaseContractFile)
        {
            try
            {
                string? payeeName = (!string.IsNullOrWhiteSpace(payeeFirstName) || !string.IsNullOrWhiteSpace(payeeLastName)) 
                    ? $"{payeeFirstName} {payeeLastName}".Trim() : null;
                    
                string? tenantName = (!string.IsNullOrWhiteSpace(tenantFirstName) || !string.IsNullOrWhiteSpace(tenantLastName)) 
                    ? $"{tenantFirstName} {tenantLastName}".Trim() : null;
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var prop = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId && p.UserId == user.Id);
            if (prop == null) return NotFound();

            prop.Dues = duesAmount;
            prop.Price = rentAmount ?? 0;
            prop.ContractStartDate = contractStartDate;
            prop.ContractEndDate = contractEndDate;
            
            // Eğer OwnerInvestment ise Kiracı Bilgileri gelir
            if (prop.ManagementRole == ManagementRole.Owner && !prop.IsResident)
            {
                prop.TenantName = tenantName;
                prop.TenantPhone = tenantPhone;
                prop.TenantEmail = tenantEmail;
                prop.TenantIdentityNumber = tenantIdentityNumber;
                prop.IsUtilitiesOnOwner = isUtilitiesOnOwner;
            }

            // Dosya yükleme (Kira Sözleşmesi)
            if (leaseContractFile != null && leaseContractFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracts");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(leaseContractFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await leaseContractFile.CopyToAsync(fileStream);
                }
                prop.LeaseContractFilePath = "/uploads/contracts/" + uniqueFileName;
            }

            prop.DraftStep = 0; // Taslaktan çıktı

            await _context.SaveChangesAsync();
            
            // Vergi ve Yükümlülükleri Otomatik Ata
            await _financialEngine.AssignObligationsToPropertyAsync(prop.Id);
            
            return Ok(new { success = true, redirectUrl = $"/DigitalHome/Details/{prop.Id}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST: Yeni Mülk Ekle (Sadece Takip Amaçlı)
        [HttpPost]
        public async Task<IActionResult> AddProperty(string title, ManagementRole role, string roomCount)
        {
            var user = await _userManager.GetUserAsync(User);
            
            // Bireysel Kullanıcı Abonelik Limiti Kontrolü
            if (user.UserType == GMK360.Core.Entities.Identity.UserType.Individual)
            {
                var existingPropertyCount = await _context.Properties.CountAsync(p => p.UserId == user.Id);
                var hasActiveSubscription = await _context.UserSubscriptions
                    .AnyAsync(s => s.UserId == user.Id && s.IsActive && s.EndDate >= DateTime.UtcNow);

                if (existingPropertyCount >= 1 && !hasActiveSubscription)
                {
                    TempData["ErrorMessage"] = "Ücretsiz kullanım limitinizi (1 Ev) doldurdunuz. Daha fazla ev eklemek veya Yapay Zeka özelliklerini kullanmak için Bireysel Premium Paketi'ne geçin.";
                    return RedirectToAction("Packages", "Subscription");
                }
            }
            
            var newProperty = new Property
            {
                Title = title,
                UserId = user.Id,
                State = ListingState.PrivateTracking,
                ManagementRole = role,
                RoomCount = roomCount,
                CreatedAt = DateTime.Now
            };

            _context.Properties.Add(newProperty);
            await _context.SaveChangesAsync();

            // Vergi ve Yükümlülükleri Otomatik Ata
            await _financialEngine.AssignObligationsToPropertyAsync(newProperty.Id);

            return RedirectToAction(nameof(Index));
        }

        // GET: Mülk Detay ve Yönetim (Dashboard)
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var property = await _context.Properties
                    .Include(p => p.Building)
                    .Include(p => p.Liabilities)
                        .ThenInclude(l => l.UtilityCompany)
                    .Include(p => p.Liabilities)
                        .ThenInclude(l => l.LiabilityType)
                    .Include(p => p.Payments)
                    .FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);

                if (property == null) return NotFound();

                // Sadece Aktif Dağıtım Firmalarını al
                ViewBag.UtilityCompanies = await _context.UtilityCompanies.Where(u => u.IsActive).OrderBy(u => u.Name).ToListAsync();
                
                // Dinamik Vergi/Yükümlülük türlerini ViewBag'e at (Tüm kurallar)
                ViewBag.ObligationTypes = await _context.FinancialObligationTypes.Where(f => f.IsActive).OrderBy(f => f.Name).ToListAsync();
                
                // Mülke atanmış sistem takvimini getir
                var schedules = await _context.PropertyFinancialSchedules
                    .Include(s => s.ObligationType)
                    .Where(s => s.PropertyId == id)
                    .OrderBy(s => s.DueDate)
                    .ToListAsync();

                // SELF-HEALING: Eğer takvim boşsa (eski kayıtlar için), otomatik oluştur
                if (!schedules.Any() && property.Type != null && property.ManagementRole != null)
                {
                    await _financialEngine.AssignObligationsToPropertyAsync(property.Id);
                    
                    // Yeniden çek
                    schedules = await _context.PropertyFinancialSchedules
                        .Include(s => s.ObligationType)
                        .Where(s => s.PropertyId == id)
                        .OrderBy(s => s.DueDate)
                        .ToListAsync();
                }
                
                ViewBag.Schedules = schedules;

                // Eğer varsa manuel girilmiş eski geçmiş (PropertyFinancialRecord)
                ViewBag.Expenses = await _context.PropertyFinancialRecords
                    .Where(f => f.PropertyId == id)
                    .OrderByDescending(f => f.DueDate)
                    .ToListAsync();

                return View(property);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Mülk detayları yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditTenant(int propertyId, string tenantFirstName, string tenantLastName, string tenantPhone, string tenantEmail, int? occupantType, decimal? rayicBedel, decimal? rentAmount)
        {
            var user = await _userManager.GetUserAsync(User);
            var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId && p.UserId == user.Id);
            
            if (property == null) return NotFound();

            property.TenantName = $"{tenantFirstName?.Trim().ToUpper()} {tenantLastName?.Trim().ToUpper()}".Trim();
            property.TenantPhone = tenantPhone?.Trim();
            property.TenantEmail = tenantEmail?.Trim();

            if (occupantType.HasValue)
            {
                property.OccupantType = (GMK360.Core.Entities.Enums.OccupantType)occupantType.Value;
                
                if (property.OccupantType == GMK360.Core.Entities.Enums.OccupantType.PayingTenant)
                {
                    property.RentAmount = rentAmount;
                    property.PropertyTaxBaseValue = null; // Kiracı varsa rayiç bedele gerek yok
                }
                else if (property.OccupantType == GMK360.Core.Entities.Enums.OccupantType.NonExemptResident)
                {
                    property.PropertyTaxBaseValue = rayicBedel;
                    property.RentAmount = null; // Emsal kira hesaplanacak
                }
                else
                {
                    // Muaf durumlar
                    property.RentAmount = 0;
                    property.PropertyTaxBaseValue = null;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = propertyId });
        }

        [HttpPost]
        public async Task<IActionResult> SaveSubscription(int propertyId, string title, int? utilityCompanyId, string referenceNumber, decimal? amount, int? dueDayOfMonth)
        {
            var user = await _userManager.GetUserAsync(User);
            var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId && p.UserId == user.Id);
            if (property == null) return NotFound();

            var liability = new GMK360.Core.Entities.PropertyLiability
            {
                PropertyId = propertyId,
                UtilityCompanyId = utilityCompanyId,
                ReferenceNumber = referenceNumber,
                Title = title,
                RecurringAmount = amount,
                DueDayOfMonth = dueDayOfMonth
            };

            _context.PropertyLiabilities.Add(liability);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = propertyId });
        }



        [HttpPost]
        public async Task<IActionResult> EditPropertySettings(
            int propertyId, string title, string roomCount, string propertyStatus, 
            int netArea, int grossArea, int bathroomCount, int? balconyCount, int? wcCount, 
            string buildingAge, int floorNumber, string isFurnished, string furnitureDetails, 
            string trackingAgentName)
        {
            var user = await _userManager.GetUserAsync(User);
            var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId && p.UserId == user.Id);
            if (property == null) return NotFound();

            property.Title = title;
            property.RoomCount = roomCount;
            property.NetArea = netArea;
            property.GrossArea = grossArea;
            property.BathroomCount = bathroomCount;
            property.BalconyCount = balconyCount;
            property.WcCount = wcCount;
            property.BuildingAge = buildingAge;
            property.FloorNumber = floorNumber;
            property.TrackingAgentName = trackingAgentName;
            
            bool parsedIsFurnished = false;
            if (!string.IsNullOrEmpty(isFurnished) && (isFurnished.ToLower() == "true" || isFurnished == "1" || isFurnished.ToLower() == "on"))
                parsedIsFurnished = true;
                
            property.IsFurnished = parsedIsFurnished;
            property.FurnitureDetails = parsedIsFurnished ? furnitureDetails : null;

            if(propertyStatus == "Passive")
            {
                // Enum yapısına göre pasife al (Örn: Sold/Passive vs.)
                // property.State = ListingState.Passive;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = propertyId });
        }

        [HttpGet]
        public async Task<IActionResult> GetOtherUnitsInBuilding(int currentPropertyId)
        {
            var user = await _userManager.GetUserAsync(User);
            var currentProp = await _context.Properties.FirstOrDefaultAsync(p => p.Id == currentPropertyId && p.UserId == user.Id);
            if (currentProp == null || !currentProp.ComplexId.HasValue) return Json(new object[] { });

            var otherUnits = await _context.Properties
                .Where(p => p.UserId == user.Id && p.ComplexId == currentProp.ComplexId && p.Id != currentProp.Id)
                .Select(p => new {
                    id = p.Id,
                    unitNumber = p.UnitNumber,
                    title = p.Title
                })
                .OrderBy(p => p.unitNumber)
                .ToListAsync();

            return Json(otherUnits);
        }

        [HttpPost]
        public async Task<IActionResult> CopyPropertySettings(int sourcePropertyId, List<int> targetPropertyIds, List<string> options)
        {
            var user = await _userManager.GetUserAsync(User);
            var source = await _context.Properties
                .Include(p => p.Features)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == sourcePropertyId && p.UserId == user.Id);

            if (source == null || targetPropertyIds == null || !targetPropertyIds.Any())
                return RedirectToAction(nameof(Details), new { id = sourcePropertyId });

            var targets = await _context.Properties
                .Include(p => p.Features)
                .Include(p => p.Images)
                .Where(p => targetPropertyIds.Contains(p.Id) && p.UserId == user.Id)
                .ToListAsync();

            bool copyPhysical = options != null && options.Contains("Physical");
            bool copyFurniture = options != null && options.Contains("Furniture");
            bool copyMedia = options != null && options.Contains("Media");
            bool copyTracking = options != null && options.Contains("Tracking");

            foreach (var target in targets)
            {
                if (copyPhysical)
                {
                    target.GrossArea = source.GrossArea;
                    target.NetArea = source.NetArea;
                    target.RoomCount = source.RoomCount;
                    target.BathroomCount = source.BathroomCount;
                    target.WcCount = source.WcCount;
                    target.BalconyCount = source.BalconyCount;
                    target.FloorNumber = source.FloorNumber;
                    target.BuildingAge = source.BuildingAge;
                }

                if (copyFurniture)
                {
                    target.IsFurnished = source.IsFurnished;
                    target.FurnitureDetails = source.FurnitureDetails;
                }

                if (copyTracking)
                {
                    target.TrackingAgentName = source.TrackingAgentName;
                }

                if (copyMedia)
                {
                    target.VideoUrl = source.VideoUrl;
                    target.VideoFilePath = source.VideoFilePath;
                    
                    if (target.Images != null && target.Images.Any())
                    {
                        _context.PropertyImages.RemoveRange(target.Images);
                    }
                    
                    target.Images = new List<PropertyImage>();
                    if (source.Images != null)
                    {
                        foreach (var img in source.Images)
                        {
                            target.Images.Add(new PropertyImage
                            {
                                ImageUrl = img.ImageUrl,
                                IsCover = img.IsCover,
                                SortOrder = img.SortOrder,
                                RoomTag = img.RoomTag,
                                IsVideo = img.IsVideo
                            });
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{targets.Count} adet daire başarıyla kopyalandı ve güncellendi.";
            return RedirectToAction(nameof(Details), new { id = sourcePropertyId });
        }

        [HttpPost]
        public async Task<IActionResult> AddExpense(int propertyId, string category, string description, decimal amount, DateTime dueDate, IFormFile document, bool applyToBuilding = false)
        {
            var user = await _userManager.GetUserAsync(User);
            var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId && p.UserId == user.Id);
            if (property == null) return NotFound();

            string documentUrl = "";

            if (document != null && document.Length > 0)
            {
                var extension = Path.GetExtension(document.FileName).ToLower();
                var fileName = Guid.NewGuid().ToString() + extension;
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
                Directory.CreateDirectory(uploadsFolder);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await document.CopyToAsync(stream);
                }

                documentUrl = $"/uploads/documents/{fileName}";
            }

            var propertiesToApply = new List<Property> { property };

            if (applyToBuilding && property.BuildingId > 0)
            {
                var otherProperties = await _context.Properties
                    .Where(p => p.UserId == user.Id && p.BuildingId == property.BuildingId && p.Id != propertyId && !p.IsDeleted)
                    .ToListAsync();
                
                propertiesToApply.AddRange(otherProperties);
            }

            foreach (var p in propertiesToApply)
            {
                var expense = new PropertyFinancialRecord
                {
                    PropertyId = p.Id,
                    ExpenseCategory = category,
                    Description = description,
                    TotalAmount = amount,
                    DueDate = dueDate,
                    DocumentUrl = documentUrl,
                    CreatedAt = DateTime.Now
                };

                _context.PropertyFinancialRecords.Add(expense);
            }

            await _context.SaveChangesAsync();

            // AI/n8n Entegrasyonu: Tadilat (Fault) bildirimiyse webhook'u tetikle
            if (category == "Tadilat")
            {
                var faultData = new
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    PropertyId = propertyId,
                    Description = description,
                    Amount = amount,
                    Date = DateTime.Now
                };
                _ = _webhookManager.SendFaultReportWebhookAsync(faultData); // Fire and forget
            }

            return RedirectToAction(nameof(Details), new { id = propertyId });
        }

        // --- TADİLAT VE İHALE MODÜLÜ ---
        
        public async Task<IActionResult> Renovations()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var requests = await _context.RenovationRequests
                .Include(r => r.Property)
                .Include(r => r.Offers)
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
                
            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRenovation(int? propertyId, string title, string description, RenovationBiddingType biddingType)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var request = new RenovationRequest
            {
                UserId = user.Id,
                PropertyId = propertyId, // Opsiyonel (Kullanıcı ev seçmeden de usta çağırabilir)
                Title = title,
                Description = description,
                BiddingType = biddingType,
                Status = RenovationStatus.Open,
                PhotoUrl = "", // TODO: Fotoğraf yükleme eklenebilir
                CreatedAt = DateTime.Now
            };

            _context.RenovationRequests.Add(request);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Renovations));
        }

        [HttpPost]
        public async Task<IActionResult> AddExternalOffer(int requestId, string providerName, decimal price, string notes)
        {
            var user = await _userManager.GetUserAsync(User);
            var request = await _context.RenovationRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == user.Id);
            if (request == null) return NotFound();

            var offer = new RenovationOffer
            {
                RenovationRequestId = requestId,
                ExternalProviderName = providerName,
                Price = price,
                Notes = notes,
                CreatedAt = DateTime.Now
            };

            _context.RenovationOffers.Add(offer);
            await _context.SaveChangesAsync();

            return RedirectToAction("RenovationDetails", new { id = requestId });
        }
        
        public async Task<IActionResult> RenovationDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var request = await _context.RenovationRequests
                .Include(r => r.Property)
                .Include(r => r.Offers)
                    .ThenInclude(o => o.ServiceProvider)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);
                
            if (request == null) return NotFound();
            
            return View(request);
        }
    }
}






