using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Core.Entities.Marketing;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Services
{
    public sealed class SubscriptionStatusSummary
    {
        public bool HasSubscription { get; set; }
        public int? SubscriptionId { get; set; }
        public int? PackageId { get; set; }
        public string PackageName { get; set; } = "Paket Yok";
        public decimal PackagePrice { get; set; }
        public decimal PricePaid { get; set; }
        public string PaymentStatus { get; set; } = "None"; // Trial, Paid, PendingPayment, Expired, Exempt, None
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DaysRemaining { get; set; }
        public bool IsExpired { get; set; }
        public int? DiscountPolicyId { get; set; }
        public string? CampaignName { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string? AdminNotes { get; set; }
    }

    public sealed class SingleUnitQuotaResult
    {
        public bool HasTcIdentity { get; set; }
        public int DistinctOwnedUnitCount { get; set; }
        public int DistinctBuildingCount { get; set; }
        public bool HasActiveMultiUnitSubscription { get; set; }
        public bool HasPropertySubscriptionStarted { get; set; }
        public bool IsPropertyDemoActive { get; set; }
        public bool IsPropertyDemoExpired { get; set; }
        public int? PropertyDemoDaysRemaining { get; set; }
        public bool HasPaidPropertyPlan { get; set; }
        public string PropertyPlanStatusLabel { get; set; } = "Demo Başlatılmadı";
        public bool CanWritePropertyData => HasPaidPropertyPlan || IsPropertyDemoActive;
        public bool IsWithinFreeSingleUnitQuota => DistinctOwnedUnitCount <= 1;
        public bool RequiresPaidUpgradeForMultiUnit => DistinctOwnedUnitCount >= 2 && !HasActiveMultiUnitSubscription;
    }

    public class SaasSubscriptionService
    {
        private readonly ApplicationDbContext _context;

        public SaasSubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public static string ResolvePrimaryModule(string? activeModules, UserType userType)
        {
            var tokens = (activeModules ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.ToLowerInvariant())
                .ToList();

            if (tokens.Contains("facility") || tokens.Contains("binayonetimi")) return "facility";
            if (tokens.Contains("demolition") || tokens.Contains("yikim")) return "demolition";
            if (tokens.Contains("construction") || tokens.Contains("insaat")) return "construction";
            if (tokens.Contains("supplier") || tokens.Contains("b2b")) return "supplier";
            if (tokens.Contains("service") || tokens.Contains("subcontractor") || userType == UserType.ServiceProvider) return "service";
            return "individual";
        }

        public static string GetModuleDisplayName(string? moduleCode)
        {
            return (moduleCode ?? "").ToLowerInvariant() switch
            {
                "facility" or "binayonetimi" => "Bina & Site Yönetimi",
                "construction" or "insaat" => "İnşaat & Şantiye",
                "demolition" or "yikim" => "Yıkım & Hafriyat",
                "excavation" => "Hafriyat",
                "supplier" or "b2b" => "Tedarik & B2B",
                "service" or "subcontractor" => "Usta & Taşeron",
                _ => "Bireysel / Mülk Sahibi"
            };
        }

        public async Task<SubscriptionPackage> EnsureDefaultPackageForModuleAsync(string primaryModule)
        {
            var (name, price, period, targetType) = primaryModule switch
            {
                "facility" => ("Bina & Site Yönetimi — Yıllık Abonelik", 5000m, PackagePeriod.Yearly, UserType.Corporate),
                "construction" => ("İnşaat & Şantiye Yönetimi — Yıllık Abonelik", 15000m, PackagePeriod.Yearly, UserType.Corporate),
                "demolition" => ("Yıkım & Hafriyat Operasyon — Yıllık Abonelik", 12000m, PackagePeriod.Yearly, UserType.Corporate),
                "supplier" => ("Tedarikçi & B2B Pazaryeri — Yıllık Abonelik", 4000m, PackagePeriod.Yearly, UserType.Corporate),
                "service" => ("Usta & Taşeron — Yıllık Abonelik", 1500m, PackagePeriod.Yearly, UserType.ServiceProvider),
                "multi_property" => ("Çoklu Mülk & Kiracı Kokpiti — Yıllık Abonelik", 1500m, PackagePeriod.Yearly, UserType.PropertyOwner),
                _ => ("Dijital Evim & Tek Mülk — Yıllık Abonelik", 490m, PackagePeriod.Yearly, UserType.Individual)
            };

            var pkg = await _context.SubscriptionPackages.FirstOrDefaultAsync(p => p.Name == name);
            if (pkg != null) return pkg;

            pkg = new SubscriptionPackage
            {
                Name = name,
                Price = price,
                Period = period,
                TargetUserType = targetType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.SubscriptionPackages.Add(pkg);
            await _context.SaveChangesAsync();
            return pkg;
        }

        public async Task<UserSubscription> AssignTrialSubscriptionAsync(
            ApplicationUser user,
            string? activeModules = null,
            int trialDays = 15,
            int? discountPolicyId = null,
            string? promoCode = null)
        {
            var primaryModule = ResolvePrimaryModule(activeModules ?? user.ActiveModules, user.UserType);
            var pkg = await EnsureDefaultPackageForModuleAsync(primaryModule);

            DiscountPolicy? policy = null;
            var now = DateTime.UtcNow;
            if (discountPolicyId.HasValue && discountPolicyId.Value > 0)
            {
                policy = await _context.DiscountPolicies.FirstOrDefaultAsync(d => d.Id == discountPolicyId.Value && d.IsActive);
            }
            else if (!string.IsNullOrWhiteSpace(promoCode))
            {
                var normCode = promoCode.Trim().ToUpperInvariant();
                policy = await _context.DiscountPolicies.FirstOrDefaultAsync(d =>
                    d.IsActive && d.PromoCode != null && d.PromoCode.ToUpper() == normCode
                    && (!d.ValidFrom.HasValue || d.ValidFrom <= now)
                    && (!d.ValidUntil.HasValue || d.ValidUntil >= now));
            }
            else
            {
                policy = await _context.DiscountPolicies
                    .Where(d => d.IsActive
                        && d.MinimumActiveSubscriptionsRequired == 0
                        && (string.IsNullOrEmpty(d.TargetModule) || d.TargetModule == "All" || d.TargetModule == primaryModule)
                        && (!d.ValidFrom.HasValue || d.ValidFrom <= now)
                        && (!d.ValidUntil.HasValue || d.ValidUntil >= now))
                    .OrderByDescending(d => d.Id)
                    .FirstOrDefaultAsync();
            }

            var existingActive = await _context.UserSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == user.Id && s.IsActive);
            if (existingActive != null)
            {
                return existingActive;
            }

            int totalDays = Math.Max(1, trialDays) + (policy?.ExtraTrialDays ?? 0);
            var sub = new UserSubscription
            {
                UserId = user.Id,
                PackageId = pkg.Id,
                StartDate = now,
                EndDate = now.AddDays(totalDays),
                IsActive = true,
                PricePaid = 0m,
                PaymentStatus = "Trial",
                DiscountPolicyId = policy?.Id,
                AdminNotes = $"{totalDays} günlük ücretsiz deneme başlatıldı.",
                CreatedAt = now
            };

            _context.UserSubscriptions.Add(sub);

            if (policy != null)
            {
                user.RegisteredCampaignId ??= policy.Id;
                _context.UserDiscounts.Add(new UserDiscount
                {
                    UserId = user.Id,
                    DiscountPolicyId = policy.Id,
                    IsUsed = false,
                    AssignedAt = now,
                    AssignedByStaffId = "system"
                });
            }

            await _context.SaveChangesAsync();
            return sub;
        }

        public static SubscriptionStatusSummary BuildStatusSummary(ApplicationUser user)
        {
            var sub = user.Subscriptions?
                .OrderByDescending(s => s.IsActive)
                .ThenByDescending(s => s.Id)
                .FirstOrDefault();

            if (sub == null)
            {
                return new SubscriptionStatusSummary
                {
                    HasSubscription = false,
                    PackageName = "Tanımlı Paket Yok",
                    PaymentStatus = "None"
                };
            }

            int? daysRemaining = null;
            bool isExpired = false;
            if (sub.EndDate.HasValue)
            {
                var diff = (int)Math.Ceiling((sub.EndDate.Value.Date - DateTime.UtcNow.Date).TotalDays);
                daysRemaining = diff;
                if (diff < 0 && !string.Equals(sub.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase) && !string.Equals(sub.PaymentStatus, "Exempt", StringComparison.OrdinalIgnoreCase))
                {
                    isExpired = true;
                }
            }

            var effectiveStatus = isExpired && string.Equals(sub.PaymentStatus, "Trial", StringComparison.OrdinalIgnoreCase)
                ? "Expired"
                : (string.IsNullOrWhiteSpace(sub.PaymentStatus) ? (sub.PricePaid > 0 ? "Paid" : "Trial") : sub.PaymentStatus);

            return new SubscriptionStatusSummary
            {
                HasSubscription = true,
                SubscriptionId = sub.Id,
                PackageId = sub.PackageId,
                PackageName = sub.Package?.Name ?? "Standart Paket",
                PackagePrice = sub.Package?.Price ?? 0m,
                PricePaid = sub.PricePaid,
                PaymentStatus = effectiveStatus,
                StartDate = sub.StartDate,
                EndDate = sub.EndDate,
                DaysRemaining = daysRemaining,
                IsExpired = isExpired,
                DiscountPolicyId = sub.DiscountPolicyId,
                CampaignName = sub.DiscountPolicy?.Name,
                DiscountPercentage = sub.DiscountPolicy?.DiscountPercentage ?? 0m,
                AdminNotes = sub.AdminNotes
            };
        }

        /// <summary>
        /// TCKN bazlı tekil "15 Gün Demo -> Tek Mülk (490 TL/Yıl) / 2+ Daire Çoklu Mülk (1.500 TL/Yıl)" kota değerlendirmesi.
        /// Aynı TCKN'ye sahip tüm hesaplardaki sahiplikleri ve deneme kullanımını birleştirir.
        /// </summary>
        public async Task<SingleUnitQuotaResult> GetLandlordFreeQuotaSummaryAsync(ApplicationUser user)
        {
            var cleanTc = GMK360.Web.Helpers.TcknHelper.Normalize(user.TcIdentityNo);
            bool hasTc = GMK360.Web.Helpers.TcknHelper.IsValidTcIdentity(cleanTc);

            var relatedUserIds = hasTc
                ? await _context.Users.AsNoTracking()
                    .Where(u => u.TcIdentityNo == cleanTc)
                    .Select(u => u.Id)
                    .ToListAsync()
                : new List<string> { user.Id };

            if (!relatedUserIds.Contains(user.Id)) relatedUserIds.Add(user.Id);

            var now = DateTime.UtcNow;
            var allSubs = await _context.UserSubscriptions
                .AsNoTracking()
                .Include(s => s.Package)
                .Where(s => relatedUserIds.Contains(s.UserId))
                .OrderByDescending(s => s.IsActive)
                .ThenByDescending(s => s.Id)
                .ToListAsync();

            bool IsPropertyPkg(UserSubscription s) =>
                s.Package != null && (s.Package.TargetUserType == UserType.Individual
                    || s.Package.TargetUserType == UserType.PropertyOwner
                    || s.Package.Name.Contains("Evim", StringComparison.OrdinalIgnoreCase)
                    || s.Package.Name.Contains("Mülk", StringComparison.OrdinalIgnoreCase));

            var propertySubs = allSubs.Where(IsPropertyPkg).ToList();
            bool hasPaidProperty = propertySubs.Any(s => s.IsActive
                && (string.Equals(s.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase) || string.Equals(s.PaymentStatus, "Exempt", StringComparison.OrdinalIgnoreCase))
                && (!s.EndDate.HasValue || s.EndDate >= now));

            var activeTrialSub = propertySubs.FirstOrDefault(s => s.IsActive
                && string.Equals(s.PaymentStatus, "Trial", StringComparison.OrdinalIgnoreCase)
                && (!s.EndDate.HasValue || s.EndDate >= now));

            bool hasUsedAnyPropertyTrial = propertySubs.Any(s => string.Equals(s.PaymentStatus, "Trial", StringComparison.OrdinalIgnoreCase));
            bool isDemoActive = activeTrialSub != null;
            bool isDemoExpired = !hasPaidProperty && !isDemoActive && hasUsedAnyPropertyTrial;
            int? demoDaysRemaining = activeTrialSub?.EndDate.HasValue == true
                ? Math.Max(0, (int)Math.Ceiling((activeTrialSub.EndDate.Value.Date - now.Date).TotalDays))
                : null;

            bool hasActiveMultiUnit = propertySubs.Any(s => s.IsActive
                && s.Package != null
                && (s.Package.TargetUserType == UserType.PropertyOwner || s.Package.Name.Contains("Çoklu", StringComparison.OrdinalIgnoreCase))
                && (string.Equals(s.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(s.PaymentStatus, "Exempt", StringComparison.OrdinalIgnoreCase)
                    || (string.Equals(s.PaymentStatus, "Trial", StringComparison.OrdinalIgnoreCase) && (!s.EndDate.HasValue || s.EndDate >= now)))
                && (!s.EndDate.HasValue || s.EndDate >= now));

            var ownedProps = await _context.Properties.AsNoTracking()
                .Where(p => !p.IsDeleted && (relatedUserIds.Contains(p.UserId) || (p.OwnerUserId != null && relatedUserIds.Contains(p.OwnerUserId))))
                .Select(p => new { p.Id, p.BuildingId })
                .ToListAsync();

            var ownedUnitLinks = await _context.BuildingUnits.AsNoTracking()
                .Where(u => !(u.Category ?? "").StartsWith("OrtakAlan")
                    && ((u.OwnerUserId != null && relatedUserIds.Contains(u.OwnerUserId))
                        || (!string.IsNullOrEmpty(user.Email) && u.OwnerEmail == user.Email)))
                .Select(u => new { u.Id, u.BuildingId, u.PropertyId })
                .ToListAsync();

            int totalDistinctUnits = ownedUnitLinks.Count + ownedProps.Count(p => !ownedUnitLinks.Any(u => u.PropertyId == p.Id));
            int distinctBuildings = ownedUnitLinks.Select(u => u.BuildingId).Concat(ownedProps.Select(p => p.BuildingId)).Where(id => id > 0).Distinct().Count();

            string statusLabel = hasPaidProperty
                ? (hasActiveMultiUnit ? "Çoklu Mülk Paketi Aktif" : "Tek Mülk Paketi Aktif")
                : (isDemoActive
                    ? $"15 Günlük Demo ({demoDaysRemaining ?? 15} gün kaldı)"
                    : (isDemoExpired ? "Demo Süresi Doldu" : "İsteğe Bağlı 15 Gün Demo"));

            return new SingleUnitQuotaResult
            {
                HasTcIdentity = hasTc,
                DistinctOwnedUnitCount = totalDistinctUnits,
                DistinctBuildingCount = distinctBuildings,
                HasActiveMultiUnitSubscription = hasActiveMultiUnit,
                HasPropertySubscriptionStarted = propertySubs.Any(),
                IsPropertyDemoActive = isDemoActive,
                IsPropertyDemoExpired = isDemoExpired,
                PropertyDemoDaysRemaining = demoDaysRemaining,
                HasPaidPropertyPlan = hasPaidProperty,
                PropertyPlanStatusLabel = statusLabel
            };
        }

        public async Task<(bool Success, string Message)> StartOnDemandPropertyDemoAsync(ApplicationUser user, bool isMultiUnit = false)
        {
            var quota = await GetLandlordFreeQuotaSummaryAsync(user);
            if (quota.HasPaidPropertyPlan)
                return (true, "Aktif ücretli mülk paketiniz zaten tanımlı.");
            if (quota.IsPropertyDemoActive)
                return (true, $"15 günlük ücretsiz demonuz zaten aktif ({quota.PropertyDemoDaysRemaining ?? 15} gün kaldı).");
            if (quota.IsPropertyDemoExpired)
                return (false, "Bu T.C. Kimlik / hesap için 15 günlük ücretsiz demo hakkı daha önce kullanılmış. Devam etmek için Tek Mülk veya Çoklu Mülk paketi seçebilirsiniz.");

            var moduleKey = (isMultiUnit || quota.DistinctOwnedUnitCount >= 2) ? "multi_property" : "individual";
            var pkg = await EnsureDefaultPackageForModuleAsync(moduleKey);
            var now = DateTime.UtcNow;

            _context.UserSubscriptions.Add(new UserSubscription
            {
                UserId = user.Id,
                PackageId = pkg.Id,
                StartDate = now,
                EndDate = now.AddDays(15),
                IsActive = true,
                PricePaid = 0m,
                PaymentStatus = "Trial",
                AdminNotes = $"İsteğe bağlı 15 günlük {(moduleKey == "multi_property" ? "Çoklu Mülk" : "Tek Mülk / Evim")} demosu başlatıldı.",
                CreatedAt = now
            });
            await _context.SaveChangesAsync();

            return (true, $"15 günlük ücretsiz {(moduleKey == "multi_property" ? "Çoklu Mülk" : "Evim & Tek Mülk")} demonuz başlatıldı!");
        }
    }
}
