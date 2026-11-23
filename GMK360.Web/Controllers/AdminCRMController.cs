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
