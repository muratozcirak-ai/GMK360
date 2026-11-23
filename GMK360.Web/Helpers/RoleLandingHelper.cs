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
            bool hasConstructionRole = hasConstruction || await userManager.IsInRoleAsync(user, "InsaatFirmasi");
            bool hasFacility = modules.Contains("facility") || await userManager.IsInRoleAsync(user, "BinaYoneticisi");
            bool hasSupplier = modules.Contains("supplier") || modules.Contains("b2b");
            bool hasService = modules.Contains("service") || modules.Contains("subcontractor") || await userManager.IsInRoleAsync(user, "Usta");
            bool hasRealEstate = modules.Contains("realestate") || await userManager.IsInRoleAsync(user, "Emlakci");
            bool hasStaff = await userManager.IsInRoleAsync(user, "Personel") || await context.BuildingStaffs.AnyAsync(s => s.UserId == user.Id && s.IsActive && !s.IsDeleted);
            bool hasResidentOrBoard = await userManager.IsInRoleAsync(user, "Sakin")
                || await context.BuildingManagers.AnyAsync(m => m.UserId == user.Id && m.IsActive && m.Role != GMK360.Core.Entities.BuildingManagerRole.HeadManager)
                || await context.BuildingUnits.AnyAsync(u => !(u.Category ?? "").StartsWith("OrtakAlan")
                    && (u.OwnerUserId == user.Id || u.TenantUserId == user.Id
                        || (!string.IsNullOrEmpty(user.Email) && (u.OwnerEmail == user.Email || u.TenantEmail == user.Email))));

            // Birden fazla is kolu / rolu olan her kullanici kendi ana sayfasina (Hub) dusup alanini secer
            int activeStreamCount = (hasConstructionRole ? 1 : 0)
                + (hasFacility ? 1 : 0)
                + (hasSupplier ? 1 : 0)
                + (hasService ? 1 : 0)
                + (hasRealEstate ? 1 : 0)
                + ((hasResidentOrBoard || hasStaff) ? 1 : 0);
            if (activeStreamCount > 1)
            {
                return "/Dashboard/Hub";
            }

            if (hasFacility)
            {
                return "/BuildingManager/Index";
            }

            if (hasConstructionRole)
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

            if (hasStaff || hasResidentOrBoard)
            {
                return "/FacilityPortal/Index";
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
