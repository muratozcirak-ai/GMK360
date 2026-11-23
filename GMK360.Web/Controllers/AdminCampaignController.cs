using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities.Marketing;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,HalklaIliskiler,Destek")]
    public class AdminCampaignController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminCampaignController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var policies = await _context.DiscountPolicies
                .Include(d => d.UserDiscounts)
                .OrderByDescending(d => d.IsActive)
                .ThenByDescending(d => d.Id)
                .ToListAsync();

            var subCounts = await _context.UserSubscriptions
                .Where(s => s.DiscountPolicyId != null && s.IsActive)
                .GroupBy(s => s.DiscountPolicyId!.Value)
                .Select(g => new { PolicyId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PolicyId, x => x.Count);

            ViewBag.ActiveSubscriptionCounts = subCounts;
            return View(policies);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePolicy(
            string name,
            string? description,
            string? targetModule,
            decimal discountPercentage,
            decimal? fixedDiscountAmount,
            int extraTrialDays,
            string? promoCode,
            DateTime? validFrom,
            DateTime? validUntil)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Kampanya adı zorunludur.";
                return RedirectToAction(nameof(Index));
            }

            var policy = new DiscountPolicy
            {
                Name = name.Trim(),
                Description = description?.Trim(),
                TargetModule = string.IsNullOrWhiteSpace(targetModule) ? "All" : targetModule.Trim(),
                DiscountPercentage = Math.Clamp(discountPercentage, 0m, 100m),
                FixedDiscountAmount = fixedDiscountAmount.HasValue && fixedDiscountAmount.Value > 0 ? fixedDiscountAmount.Value : null,
                ExtraTrialDays = Math.Max(0, extraTrialDays),
                PromoCode = string.IsNullOrWhiteSpace(promoCode) ? null : promoCode.Trim().ToUpperInvariant(),
                ValidFrom = validFrom,
                ValidUntil = validUntil,
                IsActive = true
            };

            _context.DiscountPolicies.Add(policy);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{policy.Name}' kampanyası oluşturuldu ve aktif edildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePolicy(int id)
        {
            var policy = await _context.DiscountPolicies.FindAsync(id);
            if (policy == null) return NotFound();

            policy.IsActive = !policy.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{policy.Name}' kampanyası {(policy.IsActive ? "aktif edildi" : "pasife alındı")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
