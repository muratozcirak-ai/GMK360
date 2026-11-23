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
                _ => ("Dijital Evim — Başlangıç Paketi", 0m, PackagePeriod.Free, UserType.Individual)
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
    }
}
