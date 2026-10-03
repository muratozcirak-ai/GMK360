using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Construction;
using System.Linq;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    public class SystemPhaseTemplateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SystemPhaseTemplateController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var templates = await _context.SystemPhaseTemplates.OrderBy(t => t.PhaseCategory).ThenBy(t => t.SubCategory).ToListAsync();
            return View(templates);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Add(int phaseCategory, string subCategory, string itemName)
        {
            var template = new SystemPhaseTemplate
            {
                PhaseCategory = (BudgetPhaseCategory)phaseCategory,
                SubCategory = subCategory,
                ItemName = itemName,
                IsQuoteRequired = false
            };
            _context.SystemPhaseTemplates.Add(template);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var template = await _context.SystemPhaseTemplates.FindAsync(id);
            if (template != null)
            {
                _context.SystemPhaseTemplates.Remove(template);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}