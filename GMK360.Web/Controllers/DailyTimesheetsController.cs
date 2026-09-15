using GMK360.Core.Entities.Construction;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities.Identity;
using System.Collections.Generic;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class DailyTimesheetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DailyTimesheetsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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

        public async Task<IActionResult> Index(DateTime? date)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            DateTime targetDate = date ?? DateTime.Today;
            ViewBag.TargetDate = targetDate;
            
            var timesheets = await _context.DailyTimesheets
                .Include(t => t.AgencyWorker)
                .Include(t => t.ProjectPhase).ThenInclude(p => p.ConstructionProject)
                .Where(t => t.AgencyId == agencyId.Value && t.WorkDate.Date == targetDate.Date)
                .ToListAsync();

            return View(timesheets);
        }

        public async Task<IActionResult> ProjectTimesheet(int id, DateTime? date)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var project = await _context.ConstructionProjects
                .Include(p => p.Phases)
                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId.Value);

            if (project == null) return NotFound();

            DateTime targetDate = date ?? DateTime.Today;
            ViewBag.TargetDate = targetDate;
            ViewBag.Project = project;

            var workers = await _context.AgencyWorkers
                .Where(w => w.AgencyId == agencyId.Value && w.IsActive)
                .OrderBy(w => w.FirstName).ThenBy(w => w.LastName)
                .ToListAsync();

            var existingTimesheets = await _context.DailyTimesheets
                .Where(t => t.AgencyId == agencyId.Value && t.WorkDate.Date == targetDate.Date)
                .ToDictionaryAsync(t => t.AgencyWorkerId);

            ViewBag.Workers = workers;
            ViewBag.ExistingTimesheets = existingTimesheets;

            var projects = await _context.ConstructionProjects
                .Where(p => p.AgencyId == agencyId.Value && p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Aktif_Santiye && !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToListAsync();
            ViewBag.Projects = projects;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveTimesheets(int? sourceProjectId, DateTime targetDate, int[] workerIds, int[] phaseIds, string[] statuses, string[] notesList)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            if (workerIds == null || workerIds.Length == 0)
                return RedirectToAction(nameof(ProjectTimesheet), new { id = sourceProjectId, date = targetDate.ToString("yyyy-MM-dd") });

            var workers = await _context.AgencyWorkers.Where(w => workerIds.Contains(w.Id)).ToDictionaryAsync(w => w.Id);

            for (int i = 0; i < workerIds.Length; i++)
            {
                var wId = workerIds[i];
                var status = statuses != null && i < statuses.Length ? statuses[i] : "Gelmedi";
                var note = notesList != null && i < notesList.Length ? notesList[i] : "";
                var pId = phaseIds != null && i < phaseIds.Length ? phaseIds[i] : 0; 
                
                var worker = workers.ContainsKey(wId) ? workers[wId] : null;
                if(worker == null) continue;

                var existing = await _context.DailyTimesheets
                    .FirstOrDefaultAsync(t => t.AgencyWorkerId == wId && t.WorkDate.Date == targetDate.Date);

                decimal calculatedWage = 0;
                if(status == "Tam Gün" || status == "Tam Gn") calculatedWage = worker.NetDailyWage;
                else if(status == "Yarım Gün" || status == "Yarm Gn") calculatedWage = worker.NetDailyWage / 2;

                if (existing != null)
                {
                    existing.AttendanceStatus = status;
                    existing.Notes = note;
                    existing.EarnedWage = calculatedWage;
                    
                    if (pId > 0) existing.ProjectPhaseId = pId;
                    else existing.ProjectPhaseId = null;
                }
                else
                {
                    var newTimesheet = new DailyTimesheet
                    {
                        AgencyId = agencyId.Value,
                        AgencyWorkerId = wId,
                        WorkDate = targetDate.Date,
                        ProjectPhaseId = pId > 0 ? pId : null,
                        AttendanceStatus = status,
                        EarnedWage = calculatedWage,
                        AdvancePayment = 0,
                        Notes = note
                    };
                    _context.DailyTimesheets.Add(newTimesheet);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Puantaj basariyla kaydedildi. (Maliyet ve hakedisler merkeze iletildi)";

            if (sourceProjectId.HasValue)
                return RedirectToAction(nameof(ProjectTimesheet), new { id = sourceProjectId.Value, date = targetDate.ToString("yyyy-MM-dd") });
            else
                return RedirectToAction(nameof(Index), new { date = targetDate.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        public async Task<IActionResult> RequestAdvance(int AgencyWorkerId, decimal Amount, int sourceProjectId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var worker = await _context.AgencyWorkers.FindAsync(AgencyWorkerId);
            if (worker == null) return NotFound();

            var advance = new GMK360.Core.Entities.Finance.AgencyCashTransaction
            {
                AgencyId = agencyId.Value,
                AgencyWorkerId = AgencyWorkerId,
                Amount = Amount,
                TransactionType = GMK360.Core.Entities.Finance.AgencyCashTransactionType.WorkerAdvance,
                Method = GMK360.Core.Entities.Finance.PaymentMethod.Cash,
                Status = GMK360.Core.Entities.Finance.PaymentStatus.Pending, // KASA ONAYI BEKLİYOR
                Description = $"{worker.FullName} için avans/harçlık talebi (Şantiye Onaylı)",
                PaymentDate = System.DateTime.UtcNow,
                HandledByUserId = _userManager.GetUserId(User)
            };

            _context.AgencyCashTransactions.Add(advance);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{worker.FullName} için {Amount.ToString("C2")} avans talebi oluşturuldu. Merkez onayından sonra kasadan düşülecektir.";
            return RedirectToAction("ProjectTimesheet", new { id = sourceProjectId });
        }
    }
}

