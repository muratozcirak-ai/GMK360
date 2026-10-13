using Microsoft.AspNetCore.Mvc;
using GMK360.Data.Contexts;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    public class ProjectMatrixController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectMatrixController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int projectId)
        {
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            var elevations = await _context.ProjectElevations
                .Where(e => e.ProjectId == projectId)
                .OrderBy(e => e.ParentElevationId) // Baza / Ağaç hiyerarşisi için
                .ToListAsync();

            ViewBag.Project = project;
            ViewBag.Elevations = elevations;

            return View();
        }
    }
}
