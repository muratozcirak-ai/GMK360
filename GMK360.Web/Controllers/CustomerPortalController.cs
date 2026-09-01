using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Data;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class CustomerPortalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<GMK360.Core.Entities.Identity.ApplicationUser> _userManager;

        public CustomerPortalController(ApplicationDbContext context, 
            UserManager<GMK360.Core.Entities.Identity.ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: CustomerPortal/MyUnitMaterials/5
        public async Task<IActionResult> MyUnitMaterials(int unitId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Find the unit and ensure the user owns it (or is admin)
            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                    .ThenInclude(b => b.ConstructionProject)
                        .ThenInclude(p => p.MaterialCatalogs)
                            .ThenInclude(c => c.Selections)
                .FirstOrDefaultAsync(u => u.Id == unitId);

            if (unit == null) return NotFound();

            if (!User.IsInRole("Admin") && unit.OwnerUserId != user.Id && unit.TenantUserId != user.Id)
            {
                // In a real app, maybe allow only OwnerUserId. We'll allow owner for now.
                // Just for testing flexibility, if it's not the owner, we might block them, 
                // but let's assume they can view it. We'll enforce owner.
                return Unauthorized("Sadece bu dairenin sahibi malzeme seçimi yapabilir.");
            }

            ViewBag.ProjectName = unit.Building?.ConstructionProject?.Name;
            ViewBag.BlockName = unit.Building?.BlockName;
            
            return View(unit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSelection(int unitId, int catalogId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                .FirstOrDefaultAsync(u => u.Id == unitId);

            if (unit == null || (!User.IsInRole("Admin") && unit.OwnerUserId != user.Id))
            {
                return Unauthorized();
            }

            var catalog = await _context.ProjectMaterialCatalogs.FindAsync(catalogId);
            if (catalog == null || catalog.ConstructionProjectId != unit.Building.ConstructionProjectId)
            {
                return BadRequest();
            }

            // Remove any existing selection for this category for this unit
            var existingSelections = await _context.UnitMaterialSelections
                .Include(s => s.ProjectMaterialCatalog)
                .Where(s => s.BuildingUnitId == unitId && s.ProjectMaterialCatalog.Category == catalog.Category)
                .ToListAsync();

            if (existingSelections.Any())
            {
                _context.UnitMaterialSelections.RemoveRange(existingSelections);
            }

            // Save new selection
            var selection = new UnitMaterialSelection
            {
                BuildingUnitId = unitId,
                ProjectMaterialCatalogId = catalogId,
                SelectedByUserId = user.Id,
                SelectionDate = DateTime.UtcNow,
                IsApprovedByAdmin = false
            };

            _context.UnitMaterialSelections.Add(selection);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{catalog.Category} için '{catalog.MaterialName}' seçimi başarıyla kaydedildi.";
            return RedirectToAction(nameof(MyUnitMaterials), new { unitId = unitId });
        }
    }
}
