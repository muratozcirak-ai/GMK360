using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace GMK360.Web.Controllers
{
    [Authorize]
    [Route("FinancialStrategy")]
    public class FinancialStrategyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FinancialStrategyController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Index")]
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // For now, fetch ALL construction projects
            var projects = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            // We can calculate rough historical data here for mockup purposes
            var mockHistoricalData = new List<dynamic>();
            foreach(var p in projects.Where(x => x.Status == ProjectStatus.Tamamlandi_Teslim))
            {
                // In reality, we'd sum up from ConstructionBudgetItems. 
                // For mockup, we generate fake historical data based on project settings.
            }

            return View(projects);
        }
    }
}