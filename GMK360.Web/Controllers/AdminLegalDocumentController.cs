using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    public class AdminLegalDocumentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminLegalDocumentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var templates = await _context.SystemLegalDocumentTemplates
                .OrderBy(t => t.Name)
                .ToListAsync();
            return View(templates);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SystemLegalDocumentTemplate model)
        {
            if (ModelState.IsValid)
            {
                _context.SystemLegalDocumentTemplates.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Evrak şablonu havuza eklendi.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SystemLegalDocumentTemplate model)
        {
            var existing = await _context.SystemLegalDocumentTemplates.FindAsync(model.Id);
            if (existing != null)
            {
                existing.Name = model.Name;
                existing.IssuedBy = model.IssuedBy;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Evrak şablonu güncellendi.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.SystemLegalDocumentTemplates.FindAsync(id);
            if (existing != null)
            {
                _context.SystemLegalDocumentTemplates.Remove(existing);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Evrak şablonu silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
