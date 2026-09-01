using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LegalAgreementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LegalAgreementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var agreements = await _context.LegalAgreements
                .OrderBy(x => x.Id)
                .ToListAsync();
            return View(agreements);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var agreement = await _context.LegalAgreements.FindAsync(id);
            if (agreement == null) return NotFound();
            
            return View(agreement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LegalAgreement model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var existing = await _context.LegalAgreements.FindAsync(id);
                if (existing == null) return NotFound();

                existing.Title = model.Title;
                existing.Content = model.Content;
                existing.Version = model.Version;
                existing.IsActive = model.IsActive;
                existing.IsRequired = model.IsRequired;

                _context.Update(existing);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Sözleşme başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}
