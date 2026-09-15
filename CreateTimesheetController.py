import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\TimesheetController.cs'

content = '''using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Finance;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities.Identity;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class TimesheetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TimesheetController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) return 1;
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive);
            return consultant?.AgencyId;
        }

        [HttpPost]
        public async Task<IActionResult> RequestAdvance(int AgencyWorkerId, decimal Amount, string Description)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var worker = await _context.AgencyWorkers.FirstOrDefaultAsync(w => w.Id == AgencyWorkerId && w.AgencyId == agencyId.Value);
            if (worker == null) return NotFound();

            var advance = new AgencyStaffAdvance
            {
                AgencyWorkerId = AgencyWorkerId,
                Amount = Amount,
                Description = Description,
                Status = AdvanceStatus.Pending,
                RequestDate = DateTime.UtcNow
            };

            _context.AgencyStaffAdvances.Add(advance);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Avans talebiniz merkeze iletildi. Onaylandığında otomatik düşülecektir.";
            return Redirect(Request.Headers["Referer"].ToString());
        }

        // GİZLİ LİNK (Arka Kapı)
        // Normal menüde yer almaz. Yalnızca Patron/Merkez bu linki bilerek girer.
        [HttpGet]
        public async Task<IActionResult> PendingAdvances()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var pendingList = await _context.AgencyStaffAdvances
                .Include(a => a.Worker)
                .Where(a => a.Worker != null && a.Worker.AgencyId == agencyId.Value && a.Status == AdvanceStatus.Pending)
                .OrderBy(a => a.RequestDate)
                .ToListAsync();

            return View(pendingList);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveAdvance(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var advance = await _context.AgencyStaffAdvances
                .Include(a => a.Worker)
                .FirstOrDefaultAsync(a => a.Id == id && a.Worker.AgencyId == agencyId.Value);

            if (advance == null) return NotFound();

            // Onaylandı olarak işaretle
            advance.Status = AdvanceStatus.Paid;
            advance.StatusDate = DateTime.UtcNow;

            // Kasa çıkışı yap (Ortak Kasa)
            var cashTx = new AgencyCashTransaction
            {
                AgencyId = agencyId.Value,
                TransactionType = AgencyCashTransactionType.WorkerAdvance,
                AgencyWorkerId = advance.AgencyWorkerId,
                Amount = advance.Amount,
                Method = PaymentMethod.Cash, // Varsayılan Nakit
                Description = "İşçi Avans Ödemesi: " + advance.Description,
                PaymentDate = DateTime.UtcNow,
                Status = PaymentStatus.Completed,
                HandledByUserId = _userManager.GetUserId(User)
            };

            _context.Set<AgencyCashTransaction>().Add(cashTx);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Avans onaylandı ve ortak kasadan çıkışı yapıldı.";
            return RedirectToAction(nameof(PendingAdvances));
        }
    }
}
'''
with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
