using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;

namespace GMK360.Web.Controllers
{
    // Yalnizca Super Adminlerin erisebilecegi global evrak sablonlari yonetimi
    // [Authorize(Roles = "SuperAdmin")] // Eger rol altyapiniz varsa acabilirsiniz
    public class AdminLegalDocumentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminLegalDocumentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var templates = await _context.SystemLegalDocumentTemplates.OrderBy(t => t.TargetModule).ToListAsync();
            return View(templates);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string Name, string TargetModule, string IssuedBy, bool IsMandatory, string LegalReference)
        {
            var template = new SystemLegalDocumentTemplate
            {
                Name = Name,
                TargetModule = TargetModule,
                IssuedBy = IssuedBy,
                IsMandatory = IsMandatory,
                LegalReference = LegalReference
            };
            
            _context.SystemLegalDocumentTemplates.Add(template);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var template = await _context.SystemLegalDocumentTemplates.FindAsync(id);
            if(template != null)
            {
                _context.SystemLegalDocumentTemplates.Remove(template);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
