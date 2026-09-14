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

        // GET: CustomerPortal/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Kullanicinin sahip oldugu daireleri bul
            var myUnits = await _context.BuildingUnits
                .Include(u => u.Building)
                    .ThenInclude(b => b.ConstructionProject)
                .Where(u => u.OwnerUserId == user.Id || u.TenantUserId == user.Id)
                .ToListAsync();

            return View(myUnits);
        }

        // GET: CustomerPortal/MyUnitProgress/5
        public async Task<IActionResult> MyUnitProgress(int unitId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                    .ThenInclude(b => b.ConstructionProject)
                        .ThenInclude(p => p.Phases)
                            .ThenInclude(ph => ph.PhaseTasks)
                .FirstOrDefaultAsync(u => u.Id == unitId);

            if (unit == null) return NotFound();

            if (!User.IsInRole("Admin") && unit.OwnerUserId != user.Id && unit.TenantUserId != user.Id)
            {
                return Unauthorized("Sadece bu dairenin sahibi ilerleme durumunu görebilir.");
            }

            // Insaattan son fotograflar (Task Messages icerisindeki fotograflar)
            var recentPhotos = await _context.TaskMessages
                .Include(m => m.PhaseTask)
                .Where(m => m.PhaseTask.ProjectPhase.ConstructionProjectId == unit.Building.ConstructionProjectId 
                            && !string.IsNullOrEmpty(m.PhotoUrl))
                .OrderByDescending(m => m.SentAt)
                .Take(10)
                .ToListAsync();

            ViewBag.RecentPhotos = recentPhotos;

            return View(unit);
        }

        // GET: CustomerPortal/MyUnitMaterials/5
        public async Task<IActionResult> MyUnitMaterials(int unitId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                    .ThenInclude(b => b.ConstructionProject)
                        .ThenInclude(p => p.MaterialCatalogs)
                            .ThenInclude(c => c.Selections)
                .FirstOrDefaultAsync(u => u.Id == unitId);

            if (unit == null) return NotFound();

            if (!User.IsInRole("Admin") && unit.OwnerUserId != user.Id && unit.TenantUserId != user.Id)
            {
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

            var existingSelections = await _context.UnitMaterialSelections
                .Include(s => s.ProjectMaterialCatalog)
                .Where(s => s.BuildingUnitId == unitId && s.ProjectMaterialCatalog.Category == catalog.Category)
                .ToListAsync();

            if (existingSelections.Any())
            {
                _context.UnitMaterialSelections.RemoveRange(existingSelections);
            }

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


