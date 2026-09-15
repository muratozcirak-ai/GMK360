import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\DocumentTemplateController.cs'

content = '''using GMK360.Core.Entities.Construction;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class DocumentTemplateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentTemplateController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCurrentAgencyId()
        {
            var claim = User.FindFirst("AgencyId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        public async Task<IActionResult> Index()
        {
            var agencyId = GetCurrentAgencyId();
            var templates = await _context.DocumentTemplates
                .Where(t => t.AgencyId == agencyId && !t.IsDeleted)
                .OrderBy(t => t.Category)
                .ThenBy(t => t.TemplateName)
                .ToListAsync();
            return View(templates);
        }

        public IActionResult Create()
        {
            return View(new DocumentTemplate());
        }

        [HttpPost]
        public async Task<IActionResult> Create(DocumentTemplate model)
        {
            model.AgencyId = GetCurrentAgencyId();
            model.CreatedAt = DateTime.Now;
            
            _context.DocumentTemplates.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var agencyId = GetCurrentAgencyId();
            var template = await _context.DocumentTemplates.FirstOrDefaultAsync(t => t.Id == id && t.AgencyId == agencyId);
            if (template == null) return NotFound();
            return View(template);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DocumentTemplate model)
        {
            var agencyId = GetCurrentAgencyId();
            var existing = await _context.DocumentTemplates.FirstOrDefaultAsync(t => t.Id == model.Id && t.AgencyId == agencyId);
            if (existing != null)
            {
                existing.TemplateName = model.TemplateName;
                existing.Category = model.Category;
                existing.HtmlContent = model.HtmlContent;
                existing.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var agencyId = GetCurrentAgencyId();
            var template = await _context.DocumentTemplates.FirstOrDefaultAsync(t => t.Id == id && t.AgencyId == agencyId);
            if (template != null)
            {
                template.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
'''

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
