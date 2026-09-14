using GMK360.Core.Entities.Construction;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities.Identity;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class PhaseWorkerDemandsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PhaseWorkerDemandsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) 
                return 1;
                
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var agencyIdProp = user.GetType().GetProperty("AgencyId");
                if (agencyIdProp != null)
                {
                    var val = agencyIdProp.GetValue(user);
                    if (val != null) return (int)val;
                }
            }
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var demands = await _context.PhaseWorkerDemands
                .Include(d => d.ProjectPhase)
                .ThenInclude(p => p.ConstructionProject)
                .Where(d => d.ProjectPhase.ConstructionProject.AgencyId == agencyId.Value)
                .OrderBy(d => d.TargetDate)
                .ToListAsync();

            ViewBag.Phases = await _context.ProjectPhases
                .Include(p => p.ConstructionProject)
                .Where(p => p.ConstructionProject.AgencyId == agencyId.Value)
                .ToListAsync();

            return View(demands);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PhaseWorkerDemand demand)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            demand.CreatedByUserId = _userManager.GetUserId(User) ?? "";
            demand.Status = "Açık";

            _context.PhaseWorkerDemands.Add(demand);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "İşçi talebi başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var agencyId = await GetUserAgencyIdAsync();
            var demand = await _context.PhaseWorkerDemands
                .Include(d => d.ProjectPhase)
                .ThenInclude(p => p.ConstructionProject)
                .FirstOrDefaultAsync(d => d.Id == id && d.ProjectPhase.ConstructionProject.AgencyId == agencyId);

            if (demand != null)
            {
                demand.Status = status;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Talep durumu güncellendi.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
