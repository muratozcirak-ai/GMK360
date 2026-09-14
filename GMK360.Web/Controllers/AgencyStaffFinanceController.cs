using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Identity;
using GMK360.Core.Entities.Finance;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class AgencyStaffFinanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AgencyStaffFinanceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetCurrentAgencyId()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var consultant = await _context.AgencyConsultants
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive);
                
            return consultant?.AgencyId;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestAdvance(int consultantId, decimal amount, string description)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var staff = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.Id == consultantId && c.AgencyId == agencyId);
            if (staff == null) return NotFound();

            var advance = new AgencyStaffAdvance
            {
                AgencyConsultantId = consultantId,
                Amount = amount,
                Description = description,
                RequestDate = DateTime.UtcNow,
                Status = AdvanceStatus.Pending
            };

            _context.AgencyStaffAdvances.Add(advance);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Avans talebi başarıyla oluşturuldu.";
            return RedirectToAction("Details", "AgencyStaff", new { id = consultantId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAdvance(int advanceId)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var advance = await _context.AgencyStaffAdvances
                .Include(a => a.Consultant)
                .FirstOrDefaultAsync(a => a.Id == advanceId && a.Consultant.AgencyId == agencyId);

            if (advance == null) return NotFound();

            advance.Status = AdvanceStatus.Approved;
            advance.StatusDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Avans onaylandı. Bir sonraki bordrodan otomatik düşülecektir.";
            return RedirectToAction("Details", "AgencyStaff", new { id = advance.AgencyConsultantId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateMonthlyPayroll(string period)
        {
            // period: "2026-08"
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var activeStaffs = await _context.AgencyConsultants
                .Where(c => c.AgencyId == agencyId && c.IsActive && c.MonthlySalary > 0)
                .ToListAsync();

            int generatedCount = 0;

            foreach (var staff in activeStaffs)
            {
                // Zaten bu dönem için maaş var mı?
                var existing = await _context.AgencyStaffPayrolls
                    .AnyAsync(p => p.AgencyConsultantId == staff.Id && p.Period == period && !p.IsDeleted);

                if (existing) continue;

                // Onaylanmış ama henüz bir bordroya işlenmemiş avanslar (DeductedFromPayrollId null olanlar)
                var unpaidAdvances = await _context.AgencyStaffAdvances
                    .Where(a => a.AgencyConsultantId == staff.Id && a.Status == AdvanceStatus.Approved && a.DeductedFromPayrollId == null && !a.IsDeleted)
                    .ToListAsync();

                decimal totalAdvances = unpaidAdvances.Sum(a => a.Amount);

                var payroll = new AgencyStaffPayroll
                {
                    AgencyConsultantId = staff.Id,
                    Period = period,
                    BaseSalary = staff.MonthlySalary,
                    BonusAmount = 0, // Ek prim eklenecekse sonradan edit edilebilir
                    DeductionAmount = totalAdvances,
                    NetPayableAmount = staff.MonthlySalary - totalAdvances,
                    IsPaid = false,
                    GenerationDate = DateTime.UtcNow
                };

                _context.AgencyStaffPayrolls.Add(payroll);

                // Avansları bu bordroya bağla (Navigation Property'si var ancak direkt olarak id bağlamak EF Core için daha kolay)
                foreach(var adv in unpaidAdvances)
                {
                    adv.Status = AdvanceStatus.Paid; // Bordroya yansıdı (Avans hesaptan düştü/ödendi)
                    adv.StatusDate = DateTime.UtcNow;
                    // DeductedFromPayrollId EF Core tarafından save edilince atanması gerekir ancak şu an payroll ID si yok! 
                    // Bu yüzden navigation property kullanıyoruz:
                    adv.DeductedFromPayroll = payroll; 
                }

                generatedCount++;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $" personel için  dönemi maaş tahakkuku (bordrosu) oluşturuldu.";
            return RedirectToAction("Index", "AgencyStaff"); // Toplu maaş sayfasına yönlendirilebilir.
        }
    }
}
