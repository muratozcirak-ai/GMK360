using System;
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
    public class ObligationRuleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ObligationRuleController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rules = await _context.GlobalObligationRules
                .Include(x => x.TargetExpenseCategory)
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.StartDate)
                .ToListAsync();
            return View(rules);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new GlobalObligationRule 
            { 
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GlobalObligationRule model)
        {
            if (ModelState.IsValid)
            {
                // Eğer yeni kural aktif ediliyorsa, aynı kural tipine sahip eski kuralları pasife al
                if (model.IsActive)
                {
                    var existingActiveRules = await _context.GlobalObligationRules
                        .Where(x => x.TargetExpenseCategoryId == model.TargetExpenseCategoryId && x.IsActive)
                        .ToListAsync();

                    foreach (var rule in existingActiveRules)
                    {
                        rule.IsActive = false;
                        rule.EndDate = DateTime.Today.AddDays(-1); // Dün itibariyle geçerliliği bitti
                        _context.Update(rule);
                    }
                }

                _context.GlobalObligationRules.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Yeni kural başarıyla tanımlandı.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var rule = await _context.GlobalObligationRules.FindAsync(id);
            if (rule != null)
            {
                rule.IsActive = false;
                rule.EndDate = DateTime.Today;
                _context.Update(rule);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kural başarıyla pasife alındı.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
