using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        private async Task<int> ResolveCurrentAgencyIdAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId))
            {
                var consultant = await _context.AgencyConsultants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
                if (consultant != null && consultant.AgencyId > 0)
                    return consultant.AgencyId;
            }

            var firstAgency = await _context.Agencies.AsNoTracking().OrderBy(a => a.Id).FirstOrDefaultAsync();
            return firstAgency?.Id ?? 1;
        }

        public async Task<IActionResult> ProjectTimesheet(int id, DateTime? date)
        {
            var agencyId = await ResolveCurrentAgencyIdAsync();
            var targetDate = date ?? DateTime.Today;
            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId && !p.IsDeleted);
            if (project == null) return NotFound();

            var workers = await _context.AgencyWorkers
                .Where(w => w.AgencyId == agencyId && !w.IsDeleted && w.IsActive)
                .OrderBy(w => w.FirstName)
                .ToListAsync();

            var existingTimesheets = await _context.Set<DailyTimesheet>()
                .Where(t => t.ConstructionProjectId == id && t.WorkDate.Date == targetDate.Date && !t.IsDeleted)
                .ToDictionaryAsync(t => t.AgencyWorkerId);

            var projects = await _context.ConstructionProjects
                .Where(p => p.AgencyId == agencyId && !p.IsDeleted)
                .ToListAsync();

            ViewBag.Project = project;
            ViewBag.TargetDate = targetDate;
            ViewBag.Workers = workers;
            ViewBag.ExistingTimesheets = existingTimesheets;
            ViewBag.Projects = projects;

            return View();
        }

        public async Task<IActionResult> Index(int? projectId, DateTime? workDate)
        {
            var agencyId = await ResolveCurrentAgencyIdAsync();
            var date = workDate ?? DateTime.Today;

            var agencyProjects = await _context.ConstructionProjects
                .Where(p => p.AgencyId == agencyId && !p.IsDeleted)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            // Eğer projectId seçilmemiş ama firmanın projesi varsa ilkini varsayılan seçebiliriz veya genel saha puantajı açabiliriz
            ViewBag.Projects = new SelectList(agencyProjects, "Id", "Name", projectId);
            ViewBag.CurrentDate = date.ToString("yyyy-MM-dd");
            ViewBag.CurrentProjectId = projectId;

            var workers = await _context.Set<AgencyWorker>()
                .Where(w => w.AgencyId == agencyId && !w.IsDeleted && w.IsActive)
                .OrderBy(w => w.FirstName)
                .ToListAsync();

            var existingQuery = _context.Set<DailyTimesheet>()
                .Include(t => t.AgencyWorker)
                .Include(t => t.ConstructionProject)
                .Where(t => t.AgencyId == agencyId && t.WorkDate.Date == date.Date && !t.IsDeleted);

            if (projectId.HasValue)
            {
                existingQuery = existingQuery.Where(t => t.ConstructionProjectId == projectId.Value);
            }
            else
            {
                existingQuery = existingQuery.Where(t => t.ConstructionProjectId == null);
            }

            var existingList = await existingQuery.ToListAsync();

            // Her aktif usta için o günkü puantaj satırını hazırla (yoksa taslak satır oluştur)
            var mergedRows = new List<DailyTimesheet>();
            foreach (var w in workers)
            {
                var existing = existingList.FirstOrDefault(x => x.AgencyWorkerId == w.Id);
                if (existing != null)
                {
                    existing.AgencyWorker = w;
                    mergedRows.Add(existing);
                }
                else
                {
                    mergedRows.Add(new DailyTimesheet
                    {
                        AgencyId = agencyId,
                        AgencyWorkerId = w.Id,
                        AgencyWorker = w,
                        ConstructionProjectId = projectId,
                        WorkDate = date,
                        AttendanceStatus = "Tam Gün",
                        OvertimeHours = 0,
                        EarnedWage = w.DefaultDailyWage,
                        AdvancePayment = 0,
                        PendingFieldExpense = 0
                    });
                }
            }

            if (projectId.HasValue)
            {
                var phases = await _context.Set<ConstructionBudgetItem>()
                    .Where(b => b.ConstructionProjectId == projectId.Value)
                    .Select(b => b.PhaseCategory.ToString())
                    .Distinct()
                    .ToListAsync();
                ViewBag.Phases = phases;
            }
            else
            {
                ViewBag.Phases = new List<string> { "Kaba İnşaat", "İnce İşler", "Mekanik & Elektrik", "Çevre & Saha" };
            }

            // O gün başka şantiyelerde çalışan ustaların bilgisini de uyarı/bilgi olarak gönderelim
            var otherSiteRecords = await _context.Set<DailyTimesheet>()
                .Include(t => t.ConstructionProject)
                .Where(t => t.AgencyId == agencyId && t.WorkDate.Date == date.Date && t.ConstructionProjectId != projectId && !t.IsDeleted)
                .ToListAsync();
            ViewBag.OtherSiteRecords = otherSiteRecords;

            return View(mergedRows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTimesheets(
            int? projectId,
            DateTime workDate,
            int[] workerIds,
            string[] attendance,
            decimal[]? overtimeHours,
            decimal[]? advancePayments,
            decimal[] pendingFieldExpenses,
            string[] fieldExpenseDesc,
            string[] selectedPhases)
        {
            try
            {
                var agencyId = await ResolveCurrentAgencyIdAsync();
                var workers = await _context.Set<AgencyWorker>()
                    .Where(w => w.AgencyId == agencyId && !w.IsDeleted)
                    .ToDictionaryAsync(w => w.Id);

                for (int i = 0; i < workerIds.Length; i++)
                {
                    var workerId = workerIds[i];
                    if (!workers.TryGetValue(workerId, out var worker))
                        continue;

                    var status = attendance != null && i < attendance.Length ? attendance[i] : "Tam Gün";
                    var overtime = overtimeHours != null && i < overtimeHours.Length ? overtimeHours[i] : 0m;
                    var advance = advancePayments != null && i < advancePayments.Length ? advancePayments[i] : 0m;
                    var fieldExpense = pendingFieldExpenses != null && i < pendingFieldExpenses.Length ? pendingFieldExpenses[i] : 0m;
                    var fieldDesc = fieldExpenseDesc != null && i < fieldExpenseDesc.Length ? fieldExpenseDesc[i] : "";
                    var phase = selectedPhases != null && i < selectedPhases.Length ? selectedPhases[i] : "";

                    decimal baseWage = worker.DefaultDailyWage;
                    decimal hourlyRate = baseWage / 8m;
                    decimal earnedWage = status switch
                    {
                        "Tam Gün" => baseWage + (overtime * hourlyRate * 1.5m),
                        "Yarım Gün" => (baseWage * 0.5m) + (overtime * hourlyRate * 1.5m),
                        "Bayram / Resmi Tatil (2x Çift Yevmiye)" => (baseWage * 2.0m) + (overtime * hourlyRate * 2.0m),
                        "Fazla Mesaili (1.5x)" => baseWage + ((overtime > 0 ? overtime : 2m) * hourlyRate * 1.5m),
                        "Ücretli İzin" => baseWage,
                        _ => 0m
                    };

                    var record = await _context.Set<DailyTimesheet>()
                        .FirstOrDefaultAsync(t => t.AgencyId == agencyId && t.WorkDate.Date == workDate.Date && t.AgencyWorkerId == workerId && t.ConstructionProjectId == projectId && !t.IsDeleted);

                    if (record == null)
                    {
                        record = new DailyTimesheet
                        {
                            AgencyId = agencyId,
                            AgencyWorkerId = workerId,
                            ConstructionProjectId = projectId,
                            WorkDate = workDate,
                            AttendanceStatus = status,
                            OvertimeHours = overtime,
                            EarnedWage = Math.Round(earnedWage, 2),
                            AdvancePayment = advance,
                            PhaseName = phase,
                            PendingFieldExpense = fieldExpense,
                            FieldExpenseDescription = fieldDesc,
                            Notes = fieldDesc,
                            IsMorningPresent = status != "Gelmedi",
                            IsAfternoonPresent = status == "Tam Gün" || status.Contains("Bayram") || status.Contains("Mesai"),
                            RecordedByUserId = _userManager.GetUserId(User)
                        };
                        _context.Add(record);
                    }
                    else
                    {
                        record.AttendanceStatus = status;
                        record.OvertimeHours = overtime;
                        record.EarnedWage = Math.Round(earnedWage, 2);
                        record.AdvancePayment = advance;
                        record.PhaseName = phase;
                        record.PendingFieldExpense = fieldExpense;
                        record.FieldExpenseDescription = fieldDesc;
                        record.Notes = fieldDesc;
                        record.IsMorningPresent = status != "Gelmedi";
                        record.IsAfternoonPresent = status == "Tam Gün" || status.Contains("Bayram") || status.Contains("Mesai");
                        _context.Update(record);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{workDate:dd.MM.yyyy} tarihli şantiye puantajı, bayram/fazla mesai yevmiyeleri ve avanslar başarıyla kaydedildi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index), new { projectId = projectId, workDate = workDate.ToString("yyyy-MM-dd") });
        }
    }
}