using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Helpers
{
    /// <summary>
    /// Resolves role- and module-aware landing routes for Login and PWA start_url,
    /// and repairs stale InsaatFirmasi roles on non-construction Corporate accounts.
    /// </summary>
    public static class RoleLandingHelper
    {
        private static readonly string[] ConstructionModules = { "construction", "demolition", "excavation" };

        public static bool HasConstructionModule(string? activeModules)
        {
            if (string.IsNullOrWhiteSpace(activeModules)) return false;
            var tokens = ParseModules(activeModules);
            return tokens.Any(t => ConstructionModules.Contains(t));
        }

        public static string[] ParseModules(string? activeModules)
        {
            if (string.IsNullOrWhiteSpace(activeModules)) return Array.Empty<string>();
            return activeModules
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(m => m.ToLowerInvariant())
                .Distinct()
                .ToArray();
        }

        public static async Task NormalizeCorporateRolesAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            if (user.UserType != UserType.Corporate) return;

            if (!await userManager.IsInRoleAsync(user, "Corporate"))
            {
                await userManager.AddToRoleAsync(user, "Corporate");
            }

            // Daha once Login bug'i ile BinaYoneticisi/Tedarikci hesaplarina yanlis eklenmis InsaatFirmasi rolunu temizle
            if (!HasConstructionModule(user.ActiveModules)
                && !string.IsNullOrWhiteSpace(user.ActiveModules)
                && await userManager.IsInRoleAsync(user, "InsaatFirmasi")
                && !await userManager.IsInRoleAsync(user, "Admin")
                && !await userManager.IsInRoleAsync(user, "SuperAdmin"))
            {
                await userManager.RemoveFromRoleAsync(user, "InsaatFirmasi");
            }
        }

        public static async Task<string> ResolveLandingUrlAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ApplicationUser user)
        {
            if (await userManager.IsInRoleAsync(user, "SuperAdmin") || await userManager.IsInRoleAsync(user, "Admin"))
            {
                return "/Admin";
            }

            var modules = ParseModules(user.ActiveModules);
            bool hasConstruction = HasConstructionModule(user.ActiveModules);
            bool hasFacility = modules.Contains("facility") || await userManager.IsInRoleAsync(user, "BinaYoneticisi");
            bool hasSupplier = modules.Contains("supplier") || modules.Contains("b2b");
            bool hasService = modules.Contains("service") || modules.Contains("subcontractor") || await userManager.IsInRoleAsync(user, "Usta");
            bool hasRealEstate = modules.Contains("realestate") || await userManager.IsInRoleAsync(user, "Emlakci");

            // Birden fazla ana is kolu aktifse calisma alani seciciye (Hub) gonder
            int activeStreamCount = (hasConstruction ? 1 : 0) + (hasFacility ? 1 : 0) + (hasSupplier ? 1 : 0) + (hasService ? 1 : 0) + (hasRealEstate ? 1 : 0);
            if (activeStreamCount > 1 && !hasConstruction)
            {
                return "/Dashboard/Hub";
            }

            if (hasFacility && !hasConstruction)
            {
                return "/BuildingManager/Index";
            }

            if (hasConstruction || await userManager.IsInRoleAsync(user, "InsaatFirmasi"))
            {
                return "/ConstructionProject";
            }

            if (hasSupplier)
            {
                return "/B2BPurchasing";
            }

            if (hasService)
            {
                return "/SubcontractorContract";
            }

            if (await userManager.IsInRoleAsync(user, "Sakin"))
            {
                return "/ResidentPortal/Index";
            }

            if (await userManager.IsInRoleAsync(user, "Corporate"))
            {
                return "/Dashboard/Corporate";
            }

            if (user.UserType == UserType.Individual || user.UserType == UserType.PropertyOwner)
            {
                bool hasCompletedWizard = await context.Properties.AnyAsync(p => p.UserId == user.Id);
                return hasCompletedWizard ? "/Dashboard/PropertyOwner" : "/DigitalHome/SetupWizard";
            }

            return "/Dashboard/PropertyOwner";
        }
    }
}
