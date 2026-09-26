﻿﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    public class ModuleDocumentRuleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModuleDocumentRuleController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rules = await _context.ModuleDocumentRules
                .Include(r => r.SystemLegalDocumentTemplate)
                .Include(r => r.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteTemplate)
                .OrderBy(r => r.TargetModule)
                
                .ThenBy(r => r.DisplayOrder)
                .ToListAsync();

            var templates = await _context.SystemLegalDocumentTemplates
                .OrderBy(t => t.Name)
                .ToListAsync();

            ViewBag.Templates = templates;
            ViewBag.TemplatesSelect = new SelectList(templates, "Id", "Name");
            return View(rules);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ModuleDocumentRule model, string[] PrerequisiteTemplateIdsList)
        {
            ModelState.Remove("SystemLegalDocumentTemplate");
            ModelState.Remove("Prerequisites");
            
            if (ModelState.IsValid)
            {
                _context.ModuleDocumentRules.Add(model);
                await _context.SaveChangesAsync(); // First save to get Rule Id

                if (PrerequisiteTemplateIdsList != null && PrerequisiteTemplateIdsList.Length > 0)
                {
                    var validIds = PrerequisiteTemplateIdsList.Where(id => !string.IsNullOrWhiteSpace(id)).Select(int.Parse).Distinct().ToList();
                    foreach(var prId in validIds)
                    {
                        _context.ModuleDocumentRulePrerequisites.Add(new ModuleDocumentRulePrerequisite {
                            ModuleDocumentRuleId = model.Id,
                            PrerequisiteTemplateId = prId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Kural ve ön koşulları başarıyla eklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kural eklenirken bir hata oluştu. Tüm alanları doldurduğunuzdan emin olun.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeletePrerequisite(int id)
        {
            var pr = await _context.ModuleDocumentRulePrerequisites.FindAsync(id);
            if(pr != null)
            {
                _context.ModuleDocumentRulePrerequisites.Remove(pr);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bağımlılık (Ön Koşul) başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var rule = await _context.ModuleDocumentRules.FindAsync(id);
            if (rule != null)
            {
                _context.ModuleDocumentRules.Remove(rule);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kural başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
