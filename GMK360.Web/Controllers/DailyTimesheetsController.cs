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
            
            // Sadece özet verileri getir
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

            // Projeye atanmýþ iþçiler (þimdilik tüm aktif iþçileri getiriyoruz)
            var workers = await _context.AgencyWorkers
                .Where(w => w.AgencyId == agencyId.Value && w.IsActive)
                .OrderBy(w => w.FirstName).ThenBy(w => w.LastName)
                .ToListAsync();

            // Sadece BU PROJEYE (fazlarýna) yazýlmýþ mevcut puantajlarý getir
            var existingTimesheets = await _context.DailyTimesheets
                .Where(t => t.AgencyId == agencyId.Value && t.WorkDate.Date == targetDate.Date && t.ProjectPhase != null && t.ProjectPhase.ConstructionProjectId == id)
                .ToDictionaryAsync(t => t.AgencyWorkerId);

            ViewBag.Workers = workers;
            ViewBag.ExistingTimesheets = existingTimesheets;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveTimesheets(DateTime targetDate, int[] workerIds, int[] phaseIds, string[] statuses, decimal[] wages, decimal[] advances, string[] notes, int? sourceProjectId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var userId = _userManager.GetUserId(User) ?? "";

            // Mevcut kayýtlarý al
            var existingTimesheets = await _context.DailyTimesheets
                .Where(t => t.AgencyId == agencyId.Value && t.WorkDate.Date == targetDate.Date)
                .ToListAsync();

            for (int i = 0; i < workerIds.Length; i++)
            {
                var workerId = workerIds[i];
                var status = statuses[i];
                var phaseId = phaseIds[i] > 0 ? (int?)phaseIds[i] : null;
                var wage = wages[i];
                var advance = advances[i];
                var note = notes[i];

                var existing = existingTimesheets.FirstOrDefault(t => t.AgencyWorkerId == workerId);

                // Eðer "Gelmedi" dýþýnda bir þeyse veya not/avans varsa kaydet (Gelmedi seçilince de kaydedilebilir)
                if (existing != null)
                {
                    existing.AttendanceStatus = status;
                    existing.ProjectPhaseId = phaseId;
                    existing.EarnedWage = wage;
                    existing.AdvancePayment = advance;
                    existing.Notes = note;
                    existing.RecordedByUserId = userId;
                }
                else
                {
                    if (status != "Gelmedi" || advance > 0 || !string.IsNullOrEmpty(note))
                    {
                        var newTimesheet = new DailyTimesheet
                        {
                            AgencyId = agencyId.Value,
                            AgencyWorkerId = workerId,
                            WorkDate = targetDate,
                            ProjectPhaseId = phaseId,
                            AttendanceStatus = status,
                            EarnedWage = wage,
                            AdvancePayment = advance,
                            Notes = note,
                            RecordedByUserId = userId
                        };
                        _context.DailyTimesheets.Add(newTimesheet);
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{targetDate:dd.MM.yyyy} tarihi için puantaj kayýtlarý baþarýyla güncellendi.";
            if (sourceProjectId.HasValue) return RedirectToAction(nameof(ProjectTimesheet), new { id = sourceProjectId.Value, date = targetDate.ToString("yyyy-MM-dd") });
            return RedirectToAction(nameof(Index), new { date = targetDate.ToString("yyyy-MM-dd") });
        }
    }
}



