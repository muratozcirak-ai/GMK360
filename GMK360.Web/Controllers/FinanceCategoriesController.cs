using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Finance;
using GMK360.Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class FinanceCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FinanceCategoriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            
            var consultant = await _context.Set<GMK360.Core.Entities.AgencyConsultant>().FirstOrDefaultAsync(a => a.UserId == user.Id);
            return consultant?.AgencyId;
        }

        public async Task<IActionResult> Index()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var categories = await _context.FinanceCategories
                .Where(c => c.AgencyId == agencyId.Value)
                .OrderBy(c => c.Type).ThenBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, FinanceCategoryType type, string code, bool isDefaultForMaterialReceipt, bool isDefaultForSubcontractor)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var category = new FinanceCategory
            {
                AgencyId = agencyId.Value,
                Name = name,
                Type = type,
                Code = code ?? "",
                IsDefaultForMaterialReceipt = isDefaultForMaterialReceipt,
                IsDefaultForSubcontractor = isDefaultForSubcontractor
            };

            _context.FinanceCategories.Add(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Finans kalemi başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.FinanceCategories.FindAsync(id);
            if (category != null)
            {
                _context.FinanceCategories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kalem silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

