using GMK360.Core.Entities.Construction;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities.Identity;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class AgencyWorkersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AgencyWorkersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) 
                return 1;
                
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var consultant = await _context.AgencyConsultants
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive);
                
            return consultant?.AgencyId;
        }

        public async Task<IActionResult> Index()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var workers = await _context.AgencyWorkers
                .Where(w => w.AgencyId == agencyId.Value && w.IsActive)
                .OrderBy(w => w.FirstName).ThenBy(w => w.LastName)
                .ToListAsync();

            return View(workers);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AgencyWorker worker)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            worker.AgencyId = agencyId.Value;
            worker.IsActive = true;

            _context.AgencyWorkers.Add(worker);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ýþçi/Personel baþarýyla havuza eklendi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var worker = await _context.AgencyWorkers
                .FirstOrDefaultAsync(w => w.Id == id && w.AgencyId == agencyId.Value);

            if (worker == null) return NotFound();

            // Puantaj kayýtlarýný alarak hakediþ (maaþ) ve avanslarý hesapla
            var timesheets = await _context.DailyTimesheets
                .Include(t => t.ProjectPhase).ThenInclude(p => p.ConstructionProject)
                .Where(t => t.AgencyWorkerId == id)
                .OrderByDescending(t => t.WorkDate)
                .ToListAsync();

            ViewBag.Timesheets = timesheets;
            
            // Toplam Kazanýlan Yevmiye (Hak Ediþ)
            ViewBag.TotalEarned = timesheets.Sum(t => t.EarnedWage);
            
            // Toplam Verilen Avans / Ödeme
            ViewBag.TotalAdvance = timesheets.Sum(t => t.AdvancePayment);
            
            // Kalan Alacaðý (Bakiye)
            ViewBag.CurrentBalance = ViewBag.TotalEarned - ViewBag.TotalAdvance;

            return View(worker);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            var worker = await _context.AgencyWorkers.FirstOrDefaultAsync(w => w.Id == id && w.AgencyId == agencyId);
            if (worker != null)
            {
                worker.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Personel listeden çýkarýldý.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}


