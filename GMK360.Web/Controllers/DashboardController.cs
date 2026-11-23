using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Entities.Identity; // UserType enum için
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;

using GMK360.Web.ViewModels;
using GMK360.Web.Models;
using GMK360.Core.Entities;
using GMK360.Web.Filters;
namespace GMK360.Web.Controllers
{
    [Authorize]
    public class PortalBuildingLink
    {
        public int BuildingId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        public string? BlockName { get; set; }
        public string DoorsSummary { get; set; } = string.Empty;
        public string RoleSummary { get; set; } = string.Empty;
    }

    [RequireOnboarding] public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public DashboardController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // Uygulama Merkezi (App Hub)
        public async Task<IActionResult> Hub()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.UserType == UserType.Corporate && (!User.IsInRole("Corporate") || (User.IsInRole("InsaatFirmasi") && !GMK360.Web.Helpers.RoleLandingHelper.HasConstructionModule(user.ActiveModules))))
            {
                if (!await _userManager.IsInRoleAsync(user, "Corporate"))
                {
                    await _userManager.AddToRoleAsync(user, "Corporate");
                }
                await GMK360.Web.Helpers.RoleLandingHelper.NormalizeCorporateRolesAsync(_userManager, user);
                if (GMK360.Web.Helpers.RoleLandingHelper.HasConstructionModule(user.ActiveModules) && !await _userManager.IsInRoleAsync(user, "InsaatFirmasi"))
                {
                    await _userManager.AddToRoleAsync(user, "InsaatFirmasi");
                }
                await _signInManager.RefreshSignInAsync(user);
            }

            var userUnits = user != null
                ? await _context.BuildingUnits
                    .AsNoTracking()
                    .Include(u => u.Building)
                    .Where(u => !(u.Category ?? "").StartsWith("OrtakAlan")
                        && (u.OwnerUserId == user.Id || u.TenantUserId == user.Id
                            || (!string.IsNullOrEmpty(user.Email) && (u.OwnerEmail == user.Email || u.TenantEmail == user.Email))))
                    .ToListAsync()
                : new List<BuildingUnit>();

            var boardMemberships = user != null
                ? await _context.BuildingManagers
                    .AsNoTracking()
                    .Include(m => m.Building)
                    .Where(m => m.UserId == user.Id && m.IsActive)
                    .ToListAsync()
                : new List<BuildingManager>();

            var portalGroups = userUnits
                .Where(u => u.Building != null)
                .GroupBy(u => u.BuildingId)
                .Select(g => new PortalBuildingLink
                {
                    BuildingId = g.Key,
                    BuildingName = g.First().Building.Name,
                    BlockName = g.First().Building.BlockName,
                    DoorsSummary = string.Join(", ", g.Select(x => $"No:{x.DoorNumber}")),
                    RoleSummary = g.Any(x => x.OwnerUserId == user!.Id || (!string.IsNullOrEmpty(user.Email) && x.OwnerEmail == user.Email))
                        ? (g.Any(x => x.TenantUserId == user!.Id) ? "Malik & Kiracı" : "Kat Maliki")
                        : "Kiracı"
                })
                .ToList();

            foreach (var bm in boardMemberships.Where(m => m.Building != null))
            {
                var existing = portalGroups.FirstOrDefault(p => p.BuildingId == bm.BuildingId);
                var bRole = bm.Role == BuildingManagerRole.HeadManager ? "Yönetici" : (bm.Role == BuildingManagerRole.BoardMember ? "Aza" : "Denetçi");
                if (existing != null)
                {
                    existing.RoleSummary = $"{existing.RoleSummary} + {bRole}";
                }
                else
                {
                    portalGroups.Add(new PortalBuildingLink
                    {
                        BuildingId = bm.BuildingId,
                        BuildingName = bm.Building.Name,
                        BlockName = bm.Building.BlockName,
                        DoorsSummary = "Yönetim Kurulu",
                        RoleSummary = bRole
                    });
                }
            }

            var quotaSvc = HttpContext.RequestServices.GetService<GMK360.Web.Services.SaasSubscriptionService>();
            var quotaSummary = (user != null && quotaSvc != null)
                ? await quotaSvc.GetLandlordFreeQuotaSummaryAsync(user)
                : new GMK360.Web.Services.SingleUnitQuotaResult();

            var agencyConsultant = user != null
                ? await _context.AgencyConsultants.AsNoTracking().Include(c => c.Agency).FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive)
                : null;

            ViewBag.ActiveModules = user?.ActiveModules ?? "";
            ViewBag.ResidentUnitCount = userUnits.Count;
            ViewBag.OwnerUnitCount = userUnits.Count(u => u.OwnerUserId == user!.Id || (!string.IsNullOrEmpty(user!.Email) && u.OwnerEmail == user.Email));
            ViewBag.BoardMembershipCount = boardMemberships.Count;
            ViewBag.PortalBuildingLinks = portalGroups;
            ViewBag.QuotaSummary = quotaSummary;
            ViewBag.AgencyCompanyName = agencyConsultant?.Agency?.CompanyName;
            ViewBag.HasCompletedAgency = agencyConsultant?.Agency != null && !string.IsNullOrWhiteSpace(agencyConsultant.Agency.CompanyName);
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

        public async Task<IActionResult> Construction()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            bool isAuthorized = User.IsInRole("InsaatFirmasi") || User.IsInRole("Admin") || User.IsInRole("Corporate") || user.UserType == UserType.Corporate;
            if (!isAuthorized)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var userId = user.Id;
            ViewBag.CurrentUser = user;
            var consultant = await _context.AgencyConsultants.Include(c => c.Agency).FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);
            var previewCookie = Request.Cookies["gmk360_role_preview"];
            var effectivePerms = GMK360.Web.Helpers.StaffPermissionHelper.GetEffectivePermissions(consultant?.Role, user.ActiveModules, previewCookie);
            bool hasStrategyPerm = effectivePerms.Contains(GMK360.Web.Helpers.StaffPermissionHelper.PermStrategy);

            if (consultant == null)
            {
                var fallbackAgency = await _context.Agencies.FirstOrDefaultAsync();
                ViewBag.Agency = fallbackAgency;
                ViewBag.UserRoleTitle = previewCookie == "engineer" ? "2. Rol: Teknik Ofis / Mühendis (Ön İzleme)" :
                                        previewCookie == "formen" ? "3. Rol: Saha Sorumlusu / Formen (Ön İzleme)" : "1. Rol: Patron / Firma Sahibi";
                ViewBag.IsManagerRole = hasStrategyPerm;
                return View();
            }
            var agency = consultant.Agency;
            ViewBag.Agency = agency;
            ViewBag.UserAgencyRole = consultant.Role;
            bool isManagerRole = hasStrategyPerm;
            ViewBag.IsManagerRole = isManagerRole;
            ViewBag.UserRoleTitle = previewCookie == "engineer" ? "2. Rol: Teknik Ofis / Mühendis (Ön İzleme)" :
                                    previewCookie == "formen" ? "3. Rol: Saha Sorumlusu / Formen (Ön İzleme)" :
                                    GMK360.Web.Helpers.StaffPermissionHelper.GetFriendlyRoleBadge(consultant.Role);


            agency = consultant.Agency;
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
                .Where(p => p.AgencyId == agency.Id && (p.Status == GMK360.Core.Entities.Construction.ProjectStatus.On_Gorusme_Talep || p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif) && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();
            ViewBag.TeklifProjeler = teklifProjeler;
            ViewBag.AktifSantiyeList = await _context.ConstructionProjects
                .Where(p => p.AgencyId == agency.Id && p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Aktif_Santiye && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();
            ViewBag.OpenMeetings = await _context.AgendaRecords
                .Include(a => a.Project)
                .Include(a => a.Phonebook)
                .Where(a => a.AgencyId == agency.Id && !a.IsDeleted && (a.StatusId != GMK360.Core.Entities.Construction.AgendaConstants.StatusTamamlandi || a.EventDate.Date == DateTime.Today))
                .OrderByDescending(a => a.EventDate)
                .Take(8)
                .ToListAsync();
            
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
            ViewBag.StaffCount = await _context.AgencyConsultants.CountAsync(c => c.AgencyId == agency.Id && c.IsActive);
            ViewBag.WorkerCount = await _context.AgencyWorkers.CountAsync(w => w.AgencyId == agency.Id && !w.IsDeleted);
            ViewBag.VehicleCount = await _context.CompanyVehicles.CountAsync(v => v.AgencyId == agency.Id && !v.IsDeleted);
            ViewBag.InventoryCount = await _context.InventoryItems.CountAsync(i => !i.IsDeleted && i.Warehouse != null && i.Warehouse.AgencyId == agency.Id);
            ViewBag.BankCount = await _context.Set<GMK360.Core.Entities.Finance.SupplierCurrentAccount>().CountAsync(b => !b.IsDeleted && b.AgencyId == agency.Id);
            try
            {
                var hqMetaPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"Agency_{agency.Id}", "branding", "hq_meta.json");
                if (System.IO.File.Exists(hqMetaPath))
                {
                    var hqDict = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(await System.IO.File.ReadAllTextAsync(hqMetaPath));
                    if (hqDict != null)
                    {
                        ViewBag.HqCity = hqDict.GetValueOrDefault("City", "");
                        ViewBag.HqDistrict = hqDict.GetValueOrDefault("District", "");
                        ViewBag.HqNeighborhood = hqDict.GetValueOrDefault("Neighborhood", "");
                        ViewBag.HqStreet = hqDict.GetValueOrDefault("Street", "");
                        ViewBag.HqBuildingName = hqDict.GetValueOrDefault("BuildingName", "");
                        ViewBag.HqOuterDoorNo = hqDict.GetValueOrDefault("OuterDoorNo", "");
                        ViewBag.HqInnerDoorNo = hqDict.GetValueOrDefault("InnerDoorNo", "");
                        ViewBag.HqLatitude = hqDict.GetValueOrDefault("Latitude", "");
                        ViewBag.HqLongitude = hqDict.GetValueOrDefault("Longitude", "");
                    }
                }
            }
            catch { }
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdatePersonalProfileAndPin(string firstName, string lastName, string phoneNumber, string? approvalPin)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!string.IsNullOrWhiteSpace(firstName)) user.FirstName = firstName.Trim();
            user.LastName = lastName?.Trim() ?? "";
            user.DisplayName = $"{user.FirstName} {user.LastName}".Trim();
            if (!string.IsNullOrWhiteSpace(phoneNumber)) user.PhoneNumber = phoneNumber.Trim();
            if (!string.IsNullOrWhiteSpace(approvalPin))
            {
                var cleanPin = new string(approvalPin.Where(char.IsDigit).ToArray());
                if (cleanPin.Length >= 4 && cleanPin.Length <= 6)
                {
                    user.TransactionPinHash = cleanPin;
                    user.IsTransactionPinSet = true;
                }
            }
            await _userManager.UpdateAsync(user);
            TempData["SuccessMessage"] = "Profil bilgileriniz ve işlem onay PIN ayarınız kaydedildi.";
            return RedirectToAction(nameof(Construction));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePersonalPassword(string currentPassword, string newPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["ErrorMessage"] = "Mevcut şifre ve yeni şifre alanları zorunludur.";
                return RedirectToAction(nameof(Construction));
            }
            var res = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (res.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Şifre güncellenemedi: " + string.Join(", ", res.Errors.Select(e => e.Description));
            }
            return RedirectToAction(nameof(Construction));
        }

        private IActionResult UnusedPlaceholder() {

            return View();
        }

        [Authorize(Roles = "Corporate,Admin,InsaatFirmasi,Emlakci")]
        public IActionResult Corporate()
        {
            var subdomain = HttpContext.Items["Subdomain"] as string;
            ViewBag.Subdomain = subdomain;
            return View();
        }

        [Authorize(Roles = "InsaatFirmasi,Corporate,Admin")]
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
        [Authorize(Roles = "InsaatFirmasi,Corporate,Admin")]
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

        [Authorize(Roles = "CommercialRenter,Admin,SuperAdmin")]
        public async Task<IActionResult> CommercialRenter()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isSuperAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

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

            if (facility == null && isSuperAdmin)
            {
                facility = await query.FirstOrDefaultAsync(b => 
                    b.TechnicalFeatures == "DailyRental" || (b.Description != null && b.Description.Contains("Günlük Kiralık")));
            }

            if (facility == null)
            {
                // Sıfır mock verili onboarding model
                return View(new DailyRentalWorkspaceViewModel { HasFacility = false });
            }

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
                .Take(10)
                .ToListAsync();

            var kbsSetting = await _context.KbsFacilitySettings
                .FirstOrDefaultAsync(k => k.ApplicationUserId == (facility.ManagerUserId ?? userId) && k.IsActive);

            var today = DateTime.Today;
            var model = new DailyRentalWorkspaceViewModel
            {
                HasFacility = true,
                FacilityId = facility.Id,
                FacilityName = facility.Name,
                FacilityType = facility.Description?.Contains("Pansiyon") == true ? "Butik Pansiyon" : 
                               facility.Description?.Contains("Villa") == true ? "Müstakil Villa" : "Apart Otel / Daire Grubu",
                Address = facility.Address ?? facility.StreetName ?? "Adres Tanımlı",
                KbsFacilityCode = kbsSetting?.EgmFacilityCode,
                TodayCheckInsCount = reservations.Count(r => r.StartDate.Date == today),
                TodayCheckOutsCount = reservations.Count(r => r.EndDate.Date == today),
                Units = units.Select(u => {
                    var activeRes = reservations.FirstOrDefault(r => r.UnitId == u.Id && r.StartDate.Date <= today && r.EndDate.Date >= today);
                    var firstGuest = activeRes?.Guests?.FirstOrDefault();
                    var gFullName = firstGuest != null ? $"{firstGuest.FirstName} {firstGuest.LastName}".Trim() : null;
                    return new DailyRentalUnitItemViewModel
                    {
                        Id = u.Id,
                        UnitName = string.IsNullOrWhiteSpace(u.DoorNumber) ? $"Birim #{u.Id}" : u.DoorNumber,
                        RoomLayout = u.RoomLayout ?? "Standart",
                        Capacity = u.GrossSquareMeters.HasValue && u.GrossSquareMeters.Value > 0 ? (int)u.GrossSquareMeters.Value : 2,
                        DailyRate = u.ListPrice ?? 1500m,
                        IsOccupied = !u.IsEmpty || activeRes != null,
                        CurrentGuestName = gFullName ?? activeRes?.TenantUser?.UserName,
                        CurrentCheckOutDate = activeRes?.EndDate
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

            return View(model);
        }

        [Authorize(Roles = "Supplier,Admin")]
        public IActionResult Supplier()
        {
            return View(); // Şimdilik basitçe döndürüyoruz
        }

        // --- ORTAK MODÜLLER (İleride taşınabilir veya burada kalabilir) ---

        public IActionResult Mesajlar() => View();

        [Authorize(Roles = "InsaatFirmasi,Corporate,Admin")]
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
        public async Task<IActionResult> SiteYonetimi()
        {
            var user = await _userManager.GetUserAsync(User);
            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (consultant == null) return Unauthorized();

            var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == consultant.AgencyId);
            var projects = await _context.ConstructionProjects
                .Where(p => p.AgencyId == consultant.AgencyId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.AgencyName = agency?.CompanyName ?? "Firmam";
            ViewBag.Agency = agency;
            return View(projects);
        }

        [HttpPost]
        [Authorize(Roles = "InsaatFirmasi,Corporate,Admin")]
        [IgnoreAntiforgeryToken] public async Task<IActionResult> SaveSiteDomainAndIdentity(string? subdomain, string? customDomain, string? companyName, string? taxNumber, string? authCertificateNo, string? whatsAppNumber, string? address, string? logoUrl, string? bannerUrl, string? themePrimaryColor, string? themeAccentColor, Microsoft.AspNetCore.Http.IFormFile? logoFile, string? returnUrl, string? hqCity, string? hqDistrict, string? hqNeighborhood, int? hqNeighborhoodId, string? hqStreet, string? hqBuildingName, string? hqOuterDoorNo, string? hqInnerDoorNo, string? hqLatitude, string? hqLongitude)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == user.Id);
            var agency = consultant != null
                ? await _context.Agencies.FirstOrDefaultAsync(a => a.Id == consultant.AgencyId)
                : await _context.Agencies.FirstOrDefaultAsync();
            if (agency != null)
            {
                if (!string.IsNullOrWhiteSpace(subdomain)) agency.Subdomain = subdomain.Trim().ToLowerInvariant();
                if (customDomain != null) agency.CustomDomain = customDomain.Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(companyName)) agency.CompanyName = companyName.Trim();
                if (taxNumber != null) agency.TaxNumber = taxNumber.Trim();
                if (authCertificateNo != null) agency.AuthCertificateNo = authCertificateNo.Trim();
                if (whatsAppNumber != null) agency.WhatsAppNumber = whatsAppNumber.Trim();
                if (address != null) agency.Address = address.Trim();
                if (hqNeighborhoodId.HasValue && hqNeighborhoodId.Value > 0 && !string.IsNullOrWhiteSpace(hqStreet))
                {
                    var cleanStr = hqStreet.Trim();
                    bool streetExists = await _context.Streets.AnyAsync(s => s.NeighborhoodId == hqNeighborhoodId.Value && s.Name.ToLower() == cleanStr.ToLower());
                    if (!streetExists)
                    {
                        _context.Streets.Add(new GMK360.Core.Entities.Street
                        {
                            NeighborhoodId = hqNeighborhoodId.Value,
                            Name = cleanStr
                        });
                    }
                }
                if (string.IsNullOrWhiteSpace(agency.Address) && (!string.IsNullOrWhiteSpace(hqCity) || !string.IsNullOrWhiteSpace(hqStreet) || !string.IsNullOrWhiteSpace(hqBuildingName)))
                {
                    var parts = new System.Collections.Generic.List<string>();
                    if (!string.IsNullOrWhiteSpace(hqNeighborhood)) parts.Add(hqNeighborhood.Trim());
                    if (!string.IsNullOrWhiteSpace(hqStreet)) parts.Add(hqStreet.Trim());
                    if (!string.IsNullOrWhiteSpace(hqBuildingName)) parts.Add(hqBuildingName.Trim());
                    if (!string.IsNullOrWhiteSpace(hqOuterDoorNo)) parts.Add($"No:{hqOuterDoorNo.Trim()}" + (!string.IsNullOrWhiteSpace(hqInnerDoorNo) ? $"/{hqInnerDoorNo.Trim()}" : ""));
                    if (!string.IsNullOrWhiteSpace(hqDistrict) || !string.IsNullOrWhiteSpace(hqCity)) parts.Add($"{hqDistrict?.Trim()} / {hqCity?.Trim()}".Trim(' ', '/'));
                    agency.Address = string.Join(" ", parts);
                }
                if (hqCity != null || hqDistrict != null || hqStreet != null || hqBuildingName != null || hqLatitude != null)
                {
                    try
                    {
                        var folder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"Agency_{agency.Id}", "branding");
                        System.IO.Directory.CreateDirectory(folder);
                        var metaDict = new System.Collections.Generic.Dictionary<string, string>
                        {
                            ["City"] = hqCity?.Trim() ?? "",
                            ["District"] = hqDistrict?.Trim() ?? "",
                            ["Neighborhood"] = hqNeighborhood?.Trim() ?? "",
                            ["Street"] = hqStreet?.Trim() ?? "",
                            ["BuildingName"] = hqBuildingName?.Trim() ?? "",
                            ["OuterDoorNo"] = hqOuterDoorNo?.Trim() ?? "",
                            ["InnerDoorNo"] = hqInnerDoorNo?.Trim() ?? "",
                            ["Latitude"] = hqLatitude?.Trim() ?? "",
                            ["Longitude"] = hqLongitude?.Trim() ?? ""
                        };
                        var metaJson = System.Text.Json.JsonSerializer.Serialize(metaDict, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                        await System.IO.File.WriteAllTextAsync(System.IO.Path.Combine(folder, "hq_meta.json"), metaJson);
                    }
                    catch { }
                }
                if (logoUrl != null && logoFile == null) agency.LogoUrl = logoUrl.Trim();
                if (bannerUrl != null) agency.BannerUrl = bannerUrl.Trim();
                if (!string.IsNullOrWhiteSpace(themePrimaryColor)) agency.ThemePrimaryColor = themePrimaryColor.Trim();
                if (!string.IsNullOrWhiteSpace(themeAccentColor)) agency.ThemeAccentColor = themeAccentColor.Trim();

                if (logoFile != null && logoFile.Length > 0)
                {
                    var ext = System.IO.Path.GetExtension(logoFile.FileName).ToLowerInvariant();
                    var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp", ".svg", ".bmp", ".gif" };
                    if (allowed.Contains(ext))
                    {
                        var folder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"Agency_{agency.Id}", "branding");
                        System.IO.Directory.CreateDirectory(folder);
                        var fileName = $"company_logo_{System.DateTime.Now:yyyyMMdd_HHmmss}{ext}";
                        var fullPath = System.IO.Path.Combine(folder, fileName);
                        using (var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Create))
                        {
                            await logoFile.CopyToAsync(stream);
                        }
                        agency.LogoUrl = $"/uploads/Agency_{agency.Id}/branding/{fileName}";
                    }
                }

                _context.Agencies.Update(agency);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Firma merkez künyesi, açık adresi, telefon/vergi bilgileri ve kurumsal kimlik ayarlarınız başarıyla kaydedildi.";
            }
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(SiteYonetimi));
        }

        [HttpPost]
        [Authorize(Roles = "InsaatFirmasi,Corporate,Admin")]
        public async Task<IActionResult> ToggleWebPublish(int projectId, bool isPublished)
        {
            var user = await _userManager.GetUserAsync(User);
            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (consultant == null) return Json(new { success = false });

            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == consultant.AgencyId);
            if (project != null)
            {
                project.IsPublishedOnWeb = isPublished;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}







