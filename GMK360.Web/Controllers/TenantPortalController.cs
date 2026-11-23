using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    public class TenantPortalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TenantPortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Site(string? subdomain = null)
        {
            var sub = (subdomain ?? HttpContext.Items["Subdomain"] as string ?? string.Empty).Trim().ToLowerInvariant();
            var complexId = HttpContext.Items["TenantComplexId"] as int?;
            var buildingId = HttpContext.Items["TenantBuildingId"] as int?;
            var mgmtId = HttpContext.Items["TenantManagementCompanyId"] as int?;

            HousingComplex? complex = null;
            Building? standaloneBuilding = null;
            ManagementCompany? mgmtCompany = null;
            var blocks = new List<Building>();

            if (complexId.HasValue)
            {
                complex = await _context.HousingComplexes
                    .AsNoTracking()
                    .Include(c => c.City)
                    .Include(c => c.District)
                    .Include(c => c.Neighborhood)
                    .FirstOrDefaultAsync(c => c.Id == complexId.Value);
            }
            else if (buildingId.HasValue)
            {
                standaloneBuilding = await _context.Buildings
                    .AsNoTracking()
                    .Include(b => b.City)
                    .Include(b => b.District)
                    .Include(b => b.Neighborhood)
                    .FirstOrDefaultAsync(b => b.Id == buildingId.Value);
            }
            else if (mgmtId.HasValue)
            {
                mgmtCompany = await _context.ManagementCompanies
                    .AsNoTracking()
                    .Include(m => m.ManagedComplexes)
                    .FirstOrDefaultAsync(m => m.Id == mgmtId.Value);
            }
            else if (!string.IsNullOrEmpty(sub))
            {
                complex = await _context.HousingComplexes.AsNoTracking()
                    .Include(c => c.City).Include(c => c.District).Include(c => c.Neighborhood)
                    .FirstOrDefaultAsync(c => !c.IsDeleted && c.Subdomain == sub);
                if (complex == null)
                {
                    standaloneBuilding = await _context.Buildings.AsNoTracking()
                        .Include(b => b.City).Include(b => b.District).Include(b => b.Neighborhood)
                        .FirstOrDefaultAsync(b => b.Subdomain == sub);
                }
            }

            if (complex != null)
            {
                blocks = await _context.Buildings.AsNoTracking()
                    .Where(b => b.HousingComplexId == complex.Id && b.ParentBuildingId == null)
                    .OrderBy(b => b.Id)
                    .ToListAsync();
            }
            else if (standaloneBuilding != null)
            {
                blocks.Add(standaloneBuilding);
            }

            if (complex == null && standaloneBuilding == null && mgmtCompany == null)
            {
                return RedirectToAction("Index", "Home", new { landing = "true" });
            }

            string siteTitle = complex?.Name ?? standaloneBuilding?.Name ?? mgmtCompany?.CompanyName ?? "Site Yönetimi";
            string locationLabel = complex != null
                ? $"{complex.District?.Name} / {complex.City?.Name}"
                : (standaloneBuilding != null ? $"{standaloneBuilding.District?.Name} / {standaloneBuilding.City?.Name}" : "Profesyonel Tesis Yönetimi");

            var blockIds = blocks.Select(b => b.Id).ToList();
            int totalLivingUnits = blockIds.Any()
                ? await _context.BuildingUnits.AsNoTracking()
                    .CountAsync(u => blockIds.Contains(u.BuildingId) && !(u.Category ?? "").StartsWith("OrtakAlan"))
                : 0;

            bool isManagerUser = false;
            bool isResidentOrBoardUser = false;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId) && blockIds.Any())
            {
                isManagerUser = User.IsInRole("Admin") || User.IsInRole("SuperAdmin")
                    || await _context.BuildingManagers.AsNoTracking().AnyAsync(m => blockIds.Contains(m.BuildingId) && m.UserId == userId && m.IsActive && m.Role == BuildingManagerRole.HeadManager)
                    || blocks.Any(b => b.ManagerUserId == userId);
                isResidentOrBoardUser = await _context.BuildingManagers.AsNoTracking().AnyAsync(m => blockIds.Contains(m.BuildingId) && m.UserId == userId && m.IsActive && m.Role != BuildingManagerRole.HeadManager)
                    || await _context.BuildingUnits.AsNoTracking().AnyAsync(u => blockIds.Contains(u.BuildingId) && (u.OwnerUserId == userId || u.TenantUserId == userId));
            }

            ViewBag.SiteTitle = siteTitle;
            ViewBag.Subdomain = sub;
            ViewBag.LocationLabel = locationLabel;
            ViewBag.IsIndependentSite = complex?.IsIndependentSite ?? (mgmtCompany == null);
            ViewBag.Blocks = blocks;
            ViewBag.TotalLivingUnits = totalLivingUnits;
            ViewBag.PrimaryBuildingId = blocks.FirstOrDefault()?.Id;
            ViewBag.IsManagerUser = isManagerUser;
            ViewBag.IsResidentOrBoardUser = isResidentOrBoardUser;
            ViewBag.ManagementCompany = mgmtCompany;

            return View();
        }

        [Authorize]
        public async Task<IActionResult> EnterSite()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var complexId = HttpContext.Items["TenantComplexId"] as int?;
            var buildingId = HttpContext.Items["TenantBuildingId"] as int?;

            var blockIds = new List<int>();
            if (complexId.HasValue)
            {
                blockIds = await _context.Buildings.AsNoTracking()
                    .Where(b => b.HousingComplexId == complexId.Value && b.ParentBuildingId == null)
                    .Select(b => b.Id)
                    .ToListAsync();
            }
            else if (buildingId.HasValue)
            {
                blockIds.Add(buildingId.Value);
            }

            if (blockIds.Any())
            {
                bool isHeadManager = User.IsInRole("Admin") || User.IsInRole("SuperAdmin")
                    || await _context.BuildingManagers.AsNoTracking().AnyAsync(m => blockIds.Contains(m.BuildingId) && m.UserId == userId && m.IsActive && m.Role == BuildingManagerRole.HeadManager)
                    || await _context.Buildings.AsNoTracking().AnyAsync(b => blockIds.Contains(b.Id) && b.ManagerUserId == userId);

                if (isHeadManager)
                {
                    return RedirectToAction("Index", "BuildingManager");
                }

                return RedirectToAction("Index", "FacilityPortal", new { buildingId = blockIds.First() });
            }

            return RedirectToAction("Start", "Account");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Construction(string? subdomain = null)
        {
            var sub = (subdomain ?? HttpContext.Items["Subdomain"] as string ?? string.Empty).Trim().ToLowerInvariant();
            var agencyId = HttpContext.Items["TenantAgencyId"] as int?;

            Agency? agency = null;
            if (agencyId.HasValue)
            {
                agency = await _context.Agencies.AsNoTracking().FirstOrDefaultAsync(a => a.Id == agencyId.Value && !a.IsDeleted);
            }
            else if (!string.IsNullOrEmpty(sub))
            {
                agency = await _context.Agencies.AsNoTracking().FirstOrDefaultAsync(a => a.Subdomain == sub && !a.IsDeleted);
            }

            if (agency == null)
            {
                return RedirectToAction("Index", "Home", new { landing = "true" });
            }

            var projects = await _context.ConstructionProjects
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.AgencyId == agency.Id)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            ViewBag.Agency = agency;
            ViewBag.Projects = projects;
            ViewBag.Subdomain = agency.Subdomain ?? sub;
            return View();
        }
    }
}
