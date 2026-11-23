using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,HalklaIliskiler,Destek")]
    public class AdminCRMController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GMK360.Web.Services.SaasSubscriptionService _saasService;

        public AdminCRMController(ApplicationDbContext context, GMK360.Web.Services.SaasSubscriptionService saasService)
        {
            _context = context;
            _saasService = saasService;
        }

        // --- Müşteri ve Kullanıcı Listesi (Platform Üyeleri, Modüller, 15 Gün Deneme & Ödeme Takibi) ---
        public async Task<IActionResult> Index(string? query = null, string? module = null, string? paymentStatus = null)
        {
            var usersQuery = _context.Users
                .Include(u => u.Subscriptions).ThenInclude(s => s.Package)
                .Include(u => u.Subscriptions).ThenInclude(s => s.DiscountPolicy)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim();
                usersQuery = usersQuery.Where(u =>
                    (u.FirstName != null && u.FirstName.Contains(q)) ||
                    (u.LastName != null && u.LastName.Contains(q)) ||
                    (u.DisplayName != null && u.DisplayName.Contains(q)) ||
                    (u.Email != null && u.Email.Contains(q)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(q)));
            }

            if (!string.IsNullOrWhiteSpace(module) && module != "all")
            {
                if (module == "shadow")
                {
                    usersQuery = usersQuery.Where(u => u.IsShadowAccount);
                }
                else
                {
                    var m = module.Trim().ToLower();
                    usersQuery = usersQuery.Where(u => u.ActiveModules != null && u.ActiveModules.ToLower().Contains(m));
                }
            }

            var allMatched = await usersQuery
                .OrderByDescending(u => u.Id)
                .Take(250)
                .ToListAsync();

            var filteredUsers = allMatched;
            if (!string.IsNullOrWhiteSpace(paymentStatus) && paymentStatus != "all")
            {
                filteredUsers = allMatched
                    .Where(u => GMK360.Web.Services.SaasSubscriptionService.BuildStatusSummary(u).PaymentStatus
                        .Equals(paymentStatus, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var userIds = filteredUsers.Select(u => u.Id).ToList();

            var bCounts = await _context.Buildings
                .AsNoTracking()
                .Where(b => b.ParentBuildingId == null && (userIds.Contains(b.ManagerUserId) || (b.CreatedByUserId != null && userIds.Contains(b.CreatedByUserId))))
                .Select(b => new { b.Id, UserId = b.ManagerUserId ?? b.CreatedByUserId ?? "" })
                .ToListAsync();
            var buildingCountMap = bCounts
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Id).Distinct().Count());

            var pCounts = await _context.ProjectStakeholders
                .AsNoTracking()
                .Where(ps => !ps.IsDeleted && ps.UserId != null && userIds.Contains(ps.UserId))
                .GroupBy(ps => ps.UserId!)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var allSummaries = allMatched.Select(GMK360.Web.Services.SaasSubscriptionService.BuildStatusSummary).ToList();
            ViewBag.TotalMembers = allMatched.Count;
            ViewBag.FacilityMembers = allMatched.Count(u => (u.ActiveModules ?? "").ToLower().Contains("facility"));
            ViewBag.ActiveTrialCount = allSummaries.Count(s => s.PaymentStatus == "Trial");
            ViewBag.PaidCount = allSummaries.Count(s => s.PaymentStatus == "Paid");
            ViewBag.ExpiringSoonCount = allSummaries.Count(s => s.PaymentStatus == "Trial" && s.DaysRemaining.HasValue && s.DaysRemaining.Value <= 3);

            ViewBag.BuildingCounts = buildingCountMap;
            ViewBag.ProjectCounts = pCounts;
            ViewBag.Packages = await _context.SubscriptionPackages.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
            ViewBag.Campaigns = await _context.DiscountPolicies.Where(d => d.IsActive).OrderByDescending(d => d.Id).ToListAsync();
            ViewBag.Query = query;
            ViewBag.SelectedModule = module ?? "all";
            ViewBag.SelectedPaymentStatus = paymentStatus ?? "all";

            return View(filteredUsers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartTrialForUser(string userId, int trialDays = 15, int? packageId = null, int? discountPolicyId = null, string? returnUserId = null)
        {
            var user = await _context.Users.Include(u => u.Subscriptions).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            var now = DateTime.UtcNow;
            var policy = discountPolicyId.HasValue && discountPolicyId.Value > 0
                ? await _context.DiscountPolicies.FindAsync(discountPolicyId.Value)
                : null;

            GMK360.Core.Entities.SubscriptionPackage? pkg = packageId.HasValue && packageId.Value > 0
                ? await _context.SubscriptionPackages.FindAsync(packageId.Value)
                : null;
            if (pkg == null)
            {
                var primaryModule = GMK360.Web.Services.SaasSubscriptionService.ResolvePrimaryModule(user.ActiveModules, user.UserType);
                pkg = await _saasService.EnsureDefaultPackageForModuleAsync(primaryModule);
            }

            foreach (var oldSub in user.Subscriptions.Where(s => s.IsActive))
            {
                oldSub.IsActive = false;
            }

            int totalDays = Math.Max(1, trialDays) + (policy?.ExtraTrialDays ?? 0);
            var sub = new GMK360.Core.Entities.UserSubscription
            {
                UserId = user.Id,
                PackageId = pkg.Id,
                StartDate = now,
                EndDate = now.AddDays(totalDays),
                IsActive = true,
                PricePaid = 0m,
                PaymentStatus = "Trial",
                DiscountPolicyId = policy?.Id,
                AdminNotes = $"Süper Admin tarafından {totalDays} günlük deneme başlatıldı.",
                CreatedAt = now
            };
            _context.UserSubscriptions.Add(sub);

            if (policy != null)
            {
                user.RegisteredCampaignId ??= policy.Id;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{user.FullName} için {totalDays} günlük ücretsiz deneme başlatıldı (Bitiş: {sub.EndDate:dd.MM.yyyy}).";
            if (!string.IsNullOrEmpty(returnUserId)) return RedirectToAction(nameof(UserDetails), new { id = returnUserId });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExtendSubscriptionDays(int subscriptionId, int extraDays = 15, string? returnUserId = null)
        {
            var sub = await _context.UserSubscriptions.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == subscriptionId);
            if (sub == null) return NotFound();

            var baseDate = (sub.EndDate.HasValue && sub.EndDate.Value > DateTime.UtcNow) ? sub.EndDate.Value : DateTime.UtcNow;
            sub.EndDate = baseDate.AddDays(extraDays);
            sub.IsActive = true;
            if (sub.PaymentStatus == "Expired") sub.PaymentStatus = "Trial";
            sub.AdminNotes = $"Süre +{extraDays} gün uzatıldı ({DateTime.UtcNow:dd.MM.yyyy}).";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{sub.User?.FullName} aboneliği +{extraDays} gün uzatıldı (Yeni bitiş: {sub.EndDate:dd.MM.yyyy}).";
            if (!string.IsNullOrEmpty(returnUserId)) return RedirectToAction(nameof(UserDetails), new { id = returnUserId });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSubscriptionPayment(
            int subscriptionId,
            string paymentStatus,
            decimal pricePaid,
            int? discountPolicyId,
            string? adminNotes,
            string? returnUserId = null)
        {
            var sub = await _context.UserSubscriptions.Include(s => s.User).Include(s => s.Package).FirstOrDefaultAsync(s => s.Id == subscriptionId);
            if (sub == null) return NotFound();

            sub.PaymentStatus = string.IsNullOrWhiteSpace(paymentStatus) ? "Trial" : paymentStatus.Trim();
            sub.DiscountPolicyId = discountPolicyId.HasValue && discountPolicyId.Value > 0 ? discountPolicyId.Value : null;
            sub.AdminNotes = adminNotes?.Trim();

            if (sub.PaymentStatus == "Paid")
            {
                sub.PricePaid = pricePaid > 0 ? pricePaid : (sub.Package?.Price ?? 0m);
                sub.LastPaymentDate = DateTime.UtcNow;
                sub.IsActive = true;
                if (!sub.EndDate.HasValue || sub.EndDate.Value < DateTime.UtcNow.AddDays(30))
                {
                    sub.EndDate = DateTime.UtcNow.AddYears(1);
                }
            }
            else if (sub.PaymentStatus == "Exempt")
            {
                sub.PricePaid = 0m;
                sub.IsActive = true;
            }

            if (sub.DiscountPolicyId.HasValue && sub.User != null)
            {
                sub.User.RegisteredCampaignId ??= sub.DiscountPolicyId;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{sub.User?.FullName} abonelik/ödeme durumu güncellendi ({sub.PaymentStatus}).";
            if (!string.IsNullOrEmpty(returnUserId)) return RedirectToAction(nameof(UserDetails), new { id = returnUserId });
            return RedirectToAction(nameof(Index));
        }

        // --- Kullanıcı Detay (Abonelikler ve Mülkler) ---
        public async Task<IActionResult> Detail(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var properties = await _context.Properties
                .Include(p => p.Building)
                .Where(p => p.OwnerUserId == id)
                .ToListAsync();

            ViewBag.Properties = properties;
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Tenants()
        {
            var agencies = await _context.Agencies
                .AsNoTracking()
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.Id)
                .ToListAsync();

            var agencyIds = agencies.Select(a => a.Id).ToList();
            var projectCounts = agencyIds.Any()
                ? await _context.ConstructionProjects.AsNoTracking()
                    .Where(p => !p.IsDeleted && agencyIds.Contains(p.AgencyId))
                    .GroupBy(p => p.AgencyId)
                    .Select(g => new { AgencyId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.AgencyId, x => x.Count)
                : new Dictionary<int, int>();

            var agencyOwners = agencyIds.Any()
                ? await _context.AgencyConsultants.AsNoTracking()
                    .Include(c => c.User)
                    .Where(c => agencyIds.Contains(c.AgencyId) && c.IsActive)
                    .GroupBy(c => c.AgencyId)
                    .Select(g => new { AgencyId = g.Key, Email = g.First().User != null ? g.First().User.Email : "" })
                    .ToDictionaryAsync(x => x.AgencyId, x => x.Email ?? "")
                : new Dictionary<int, string>();

            var complexes = await _context.HousingComplexes
                .AsNoTracking()
                .Include(c => c.City)
                .Include(c => c.District)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Id)
                .ToListAsync();

            var allBuildings = await _context.Buildings
                .AsNoTracking()
                .Include(b => b.ManagerUser)
                .Include(b => b.City)
                .Include(b => b.District)
                .Where(b => b.ParentBuildingId == null)
                .OrderBy(b => b.Id)
                .ToListAsync();

            var unitCountsByBuilding = await _context.BuildingUnits.AsNoTracking()
                .Where(u => !(u.Category ?? "").StartsWith("OrtakAlan"))
                .GroupBy(u => u.BuildingId)
                .Select(g => new { BuildingId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BuildingId, x => x.Count);

            var standaloneBuildings = allBuildings.Where(b => !b.HousingComplexId.HasValue).ToList();

            ViewBag.Agencies = agencies;
            ViewBag.ProjectCounts = projectCounts;
            ViewBag.AgencyOwners = agencyOwners;
            ViewBag.Complexes = complexes;
            ViewBag.AllRootBuildings = allBuildings;
            ViewBag.StandaloneBuildings = standaloneBuildings;
            ViewBag.UnitCountsByBuilding = unitCountsByBuilding;
            return View();
        }

        private static string NormalizeSubdomainSlug(string? raw, string fallbackPrefix)
        {
            if (string.IsNullOrWhiteSpace(raw)) raw = fallbackPrefix;
            var s = raw.Trim().ToLowerInvariant()
                .Replace("ç", "c").Replace("ğ", "g").Replace("ı", "i")
                .Replace("ö", "o").Replace("ş", "s").Replace("ü", "u")
                .Replace(" ", "-");
            s = System.Text.RegularExpressions.Regex.Replace(s, @"[^a-z0-9\-]", "");
            s = System.Text.RegularExpressions.Regex.Replace(s, @"-{2,}", "-").Trim('-');
            if (s.Length > 35) s = s.Substring(0, 35).Trim('-');
            return string.IsNullOrWhiteSpace(s) ? fallbackPrefix : s;
        }

        private async Task<bool> IsSubdomainTakenAsync(string slug, int? excludeAgencyId = null, int? excludeComplexId = null, int? excludeBuildingId = null)
        {
            return await _context.Agencies.AnyAsync(a => !a.IsDeleted && a.Subdomain == slug && (!excludeAgencyId.HasValue || a.Id != excludeAgencyId.Value))
                || await _context.HousingComplexes.AnyAsync(c => !c.IsDeleted && c.Subdomain == slug && (!excludeComplexId.HasValue || c.Id != excludeComplexId.Value))
                || await _context.Buildings.AnyAsync(b => b.Subdomain == slug && (!excludeBuildingId.HasValue || b.Id != excludeBuildingId.Value) && (!excludeComplexId.HasValue || b.HousingComplexId != excludeComplexId.Value))
                || await _context.ManagementCompanies.AnyAsync(m => !m.IsDeleted && m.Subdomain == slug);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAgencyBranding(
            int agencyId,
            string companyName,
            string? subdomain,
            string? customDomain,
            string? whatsAppNumber,
            string? taxNumber,
            string? address,
            string? themePrimaryColor,
            string? themeAccentColor,
            string? bannerUrl,
            Microsoft.AspNetCore.Http.IFormFile? logoFile)
        {
            var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == agencyId && !a.IsDeleted);
            if (agency == null)
            {
                TempData["ErrorMessage"] = "Firma kaydı bulunamadı.";
                return RedirectToAction(nameof(Tenants));
            }

            string cleanSub = NormalizeSubdomainSlug(subdomain, $"firma{agency.Id}");
            if (await IsSubdomainTakenAsync(cleanSub, excludeAgencyId: agency.Id))
            {
                TempData["ErrorMessage"] = $"'{cleanSub}.gmk360.com' adresi başka bir kurum/site tarafından kullanılıyor.";
                return RedirectToAction(nameof(Tenants));
            }

            if (!string.IsNullOrWhiteSpace(companyName)) agency.CompanyName = companyName.Trim();
            agency.Subdomain = cleanSub;
            agency.CustomDomain = string.IsNullOrWhiteSpace(customDomain) ? "-" : customDomain.Trim().ToLowerInvariant();
            agency.WhatsAppNumber = string.IsNullOrWhiteSpace(whatsAppNumber) ? "-" : whatsAppNumber.Trim();
            agency.TaxNumber = string.IsNullOrWhiteSpace(taxNumber) ? "-" : taxNumber.Trim();
            agency.Address = string.IsNullOrWhiteSpace(address) ? "-" : address.Trim();
            if (!string.IsNullOrWhiteSpace(themePrimaryColor)) agency.ThemePrimaryColor = themePrimaryColor.Trim();
            if (!string.IsNullOrWhiteSpace(themeAccentColor)) agency.ThemeAccentColor = themeAccentColor.Trim();
            if (!string.IsNullOrWhiteSpace(bannerUrl)) agency.BannerUrl = bannerUrl.Trim();

            if (logoFile != null && logoFile.Length > 0 && logoFile.Length <= 5 * 1024 * 1024)
            {
                var ext = System.IO.Path.GetExtension(logoFile.FileName)?.ToLowerInvariant();
                if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".webp" || ext == ".svg")
                {
                    var relDir = System.IO.Path.Combine("uploads", "agencies");
                    var absDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", relDir);
                    System.IO.Directory.CreateDirectory(absDir);
                    var fileName = $"agency-{agency.Id}-{Guid.NewGuid():N}{ext}";
                    using (var fs = new System.IO.FileStream(System.IO.Path.Combine(absDir, fileName), System.IO.FileMode.Create))
                    {
                        await logoFile.CopyToAsync(fs);
                    }
                    agency.LogoUrl = $"/uploads/agencies/{fileName}";
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"'{agency.CompanyName}' marka ve alt domain (https://{cleanSub}.gmk360.com) ayarları güncellendi.";
            return RedirectToAction(nameof(Tenants));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSiteBranding(
            int? complexId,
            int? buildingId,
            string siteName,
            string? subdomain,
            bool isIndependentSite = true)
        {
            string cleanSub = string.IsNullOrWhiteSpace(subdomain) ? "" : NormalizeSubdomainSlug(subdomain, "site");
            if (!string.IsNullOrEmpty(cleanSub) && await IsSubdomainTakenAsync(cleanSub, excludeComplexId: complexId, excludeBuildingId: buildingId))
            {
                TempData["ErrorMessage"] = $"'{cleanSub}.gmk360.com' adresi başka bir kurum/site tarafından kullanılıyor.";
                return RedirectToAction(nameof(Tenants));
            }

            if (complexId.HasValue && complexId.Value > 0)
            {
                var complex = await _context.HousingComplexes.FirstOrDefaultAsync(c => c.Id == complexId.Value);
                if (complex != null)
                {
                    if (!string.IsNullOrWhiteSpace(siteName)) complex.Name = siteName.Trim();
                    complex.Subdomain = string.IsNullOrEmpty(cleanSub) ? null : cleanSub;
                    complex.IsIndependentSite = isIndependentSite;
                    var blocks = await _context.Buildings.Where(b => b.HousingComplexId == complex.Id && b.ParentBuildingId == null).ToListAsync();
                    foreach (var b in blocks) b.Subdomain = complex.Subdomain;
                }
            }
            else if (buildingId.HasValue && buildingId.Value > 0)
            {
                var bld = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == buildingId.Value);
                if (bld != null)
                {
                    if (!string.IsNullOrWhiteSpace(siteName)) bld.Name = siteName.Trim();
                    bld.Subdomain = string.IsNullOrEmpty(cleanSub) ? null : cleanSub;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Site kimliği güncellendi" + (!string.IsNullOrEmpty(cleanSub) ? $" (https://{cleanSub}.gmk360.com)." : ".");
            return RedirectToAction(nameof(Tenants));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutoAssignMissingSubdomains()
        {
            int updatedCount = 0;

            var complexes = await _context.HousingComplexes.Where(c => !c.IsDeleted && string.IsNullOrEmpty(c.Subdomain)).ToListAsync();
            foreach (var c in complexes)
            {
                string slug = NormalizeSubdomainSlug(c.Name, $"site{c.Id}");
                if (await IsSubdomainTakenAsync(slug, excludeComplexId: c.Id)) slug = $"{slug}-{c.Id}";
                c.Subdomain = slug;
                updatedCount++;
            }

            var standalone = await _context.Buildings.Where(b => b.HousingComplexId == null && b.ParentBuildingId == null && string.IsNullOrEmpty(b.Subdomain)).ToListAsync();
            foreach (var b in standalone)
            {
                string slug = NormalizeSubdomainSlug(b.Name, $"bina{b.Id}");
                if (await IsSubdomainTakenAsync(slug, excludeBuildingId: b.Id)) slug = $"{slug}-{b.Id}";
                b.Subdomain = slug;
                updatedCount++;
            }

            var agencies = await _context.Agencies.Where(a => !a.IsDeleted && (string.IsNullOrEmpty(a.Subdomain) || a.Subdomain == "-")).ToListAsync();
            foreach (var a in agencies)
            {
                string slug = NormalizeSubdomainSlug(a.CompanyName, $"firma{a.Id}");
                if (await IsSubdomainTakenAsync(slug, excludeAgencyId: a.Id)) slug = $"{slug}-{a.Id}";
                a.Subdomain = slug;
                updatedCount++;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = updatedCount > 0
                ? $"Eksik alt domaini olan {updatedCount} kurum/site için temiz alt domain otomatik atandı."
                : "Tüm mevcut kurum ve sitelerin alt domaini zaten tanımlı.";
            return RedirectToAction(nameof(Tenants));
        }

        // --- Kapsamlı Profil ve İlişkiler Ekranı ---
        public async Task<IActionResult> UserDetails(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id)) return NotFound();

                var user = await _context.Users
                    .Include(u => u.Subscriptions).ThenInclude(s => s.Package)
                    .Include(u => u.Subscriptions).ThenInclude(s => s.DiscountPolicy)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Index");
                }

                var projects = await _context.ProjectStakeholders
                    .Include(ps => ps.Project)
                    .Where(ps => ps.UserId == id && !ps.IsDeleted)
                    .ToListAsync();

                var agencyRoles = await _context.AgencyConsultants
                    .Include(ac => ac.Agency)
                    .Where(ac => ac.UserId == id && !ac.IsDeleted)
                    .ToListAsync();

                var managedBuildings = await _context.Buildings
                    .Include(b => b.HousingComplex)
                    .Where(b => b.ParentBuildingId == null && (b.ManagerUserId == id || b.CreatedByUserId == id || b.BuildingManagers.Any(m => m.UserId == id && m.IsActive)))
                    .OrderByDescending(b => b.Id)
                    .ToListAsync();

                ViewBag.Projects = projects;
                ViewBag.AgencyRoles = agencyRoles;
                ViewBag.ManagedBuildings = managedBuildings;
                ViewBag.Packages = await _context.SubscriptionPackages.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
                ViewBag.Campaigns = await _context.DiscountPolicies.Where(d => d.IsActive).OrderByDescending(d => d.Id).ToListAsync();

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kullanıcı detayları yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
