using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GMK360.Web.Controllers
{
    [Authorize]
    [Route("DailyTimesheet")]
    public class DailyTimesheetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DailyTimesheetController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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

        [HttpGet("Project/{projectId}")]
        public async Task<IActionResult> DailyCheckin(int projectId, string date = null)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var targetDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value);
            if (project == null) return NotFound("Proje bulunamadı.");

            var allWorkers = await _context.AgencyWorkers
                .Where(w => w.AgencyId == agencyId.Value && w.IsActive)
                .OrderBy(w => w.WorkerType)
                .ThenBy(w => w.ForemanId.HasValue ? w.ForemanId.Value : w.Id)
                .ToListAsync();

            var todayTimesheets = await _context.DailyTimesheets
                .Where(t => t.ConstructionProjectId == projectId && t.WorkDate.Date == targetDate.Date)
                .ToListAsync();

            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectId = projectId;
            ViewBag.TargetDate = targetDate.ToString("yyyy-MM-dd");
            ViewBag.Timesheets = todayTimesheets;
            
            var phases = await _context.ProjectLegalDocuments
                .Where(d => d.ConstructionProjectId == projectId)
                .Select(d => d.Stage)
                .Distinct()
                .ToListAsync();
            
            ViewBag.Phases = phases;

            ViewBag.ExistingTeams = allWorkers.Where(w => !string.IsNullOrEmpty(w.TeamName)).Select(w => w.TeamName).Distinct().ToList();
            ViewBag.ExistingSubcontractors = allWorkers.Where(w => !string.IsNullOrEmpty(w.SubcontractorName)).Select(w => w.SubcontractorName).Distinct().ToList();

            return View(allWorkers);
        }

        [HttpPost("QuickAddWorker")]
        public async Task<IActionResult> QuickAddWorker(
            [FromForm] int projectId, 
            [FromForm] string firstName, 
            [FromForm] string lastName, 
            [FromForm] string identityNumber, 
            [FromForm] string profession, 
            [FromForm] string teamName, 
            [FromForm] decimal dailyWage,
            [FromForm] string workerType,
            [FromForm] string subcontractorName)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            bool isSubcontractor = workerType == "Taşeron Personeli";

            var worker = new AgencyWorker
            {
                AgencyId = agencyId.Value,
                FirstName = firstName,
                LastName = lastName,
                IdentityNumber = identityNumber ?? "Bilinmiyor",
                PhoneNumber = "Bilinmiyor",
                Profession = profession ?? (isSubcontractor ? "Taşeron İşi" : "Düz İşçi"),
                TeamName = isSubcontractor ? null : (string.IsNullOrEmpty(teamName) ? "Bağımsız Çalışanlar" : teamName),
                SubcontractorName = isSubcontractor ? subcontractorName : null,
                WorkerType = isSubcontractor ? "Taşeron Personeli" : "Firma Personeli",
                DefaultDailyWage = dailyWage, // Taşeronun bize günlük maliyeti (veya sıfır)
                IsActive = true
            };

            _context.AgencyWorkers.Add(worker);
            await _context.SaveChangesAsync();

            return RedirectToAction("DailyCheckin", new { projectId = projectId });
        }

        [HttpPost("SaveDaily")]
        public async Task<IActionResult> SaveDaily([FromBody] TimesheetSaveRequest request)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var targetDate = DateTime.Parse(request.Date);
            var userId = _userManager.GetUserId(User);

            foreach (var record in request.Records)
            {
                var worker = await _context.AgencyWorkers.FirstOrDefaultAsync(w => w.Id == record.WorkerId && w.AgencyId == agencyId.Value);
                if (worker == null) continue;

                var existingSheet = await _context.DailyTimesheets
                    .FirstOrDefaultAsync(t => t.ConstructionProjectId == request.ProjectId && t.AgencyWorkerId == record.WorkerId && t.WorkDate.Date == targetDate.Date);

                if (existingSheet == null)
                {
                    existingSheet = new DailyTimesheet
                    {
                        AgencyId = agencyId.Value,
                        ConstructionProjectId = request.ProjectId,
                        AgencyWorkerId = record.WorkerId,
                        WorkDate = targetDate.Date,
                        RecordedByUserId = userId
                    };
                    _context.DailyTimesheets.Add(existingSheet);
                }

                existingSheet.IsMorningPresent = record.IsMorning;
                existingSheet.IsAfternoonPresent = record.IsAfternoon;
                existingSheet.OvertimeHours = record.Overtime;
                existingSheet.PhaseName = record.Phase;

                decimal wageMultiplier = 0;
                string status = "Bekliyor";

                if (existingSheet.IsMorningPresent && existingSheet.IsAfternoonPresent)
                {
                    wageMultiplier = 1.0m; 
                    status = "Tam Gün";
                }
                else if (existingSheet.IsMorningPresent || existingSheet.IsAfternoonPresent)
                {
                    wageMultiplier = 0.5m; 
                    status = "Yarım Gün";
                }
                else
                {
                    wageMultiplier = 0m; 
                    status = "Gelmedi";
                }

                if (existingSheet.OvertimeHours > 0)
                {
                    decimal hourlyRate = (worker.DefaultDailyWage / 8m) * 1.5m;
                    decimal overtimePay = existingSheet.OvertimeHours * hourlyRate;
                    existingSheet.EarnedWage = (worker.DefaultDailyWage * wageMultiplier) + overtimePay;
                    status += $" (+{existingSheet.OvertimeHours} Saat Mesai)";
                }
                else
                {
                    existingSheet.EarnedWage = worker.DefaultDailyWage * wageMultiplier;
                }
                
                existingSheet.AttendanceStatus = status;
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Puantaj kaydedildi." });
        }
        
        [HttpGet("GlobalSummary")]
        public async Task<IActionResult> GlobalSummary(string date = null)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var targetDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

            // Tüm aktif projeleri al
            var activeProjects = await _context.ConstructionProjects
                .Where(p => p.AgencyId == agencyId.Value)
                .ToListAsync();

            var projectSummaries = new List<dynamic>();
            int grandTotalFirmWorkers = 0;
            int grandTotalSubcontractors = 0;
            int grandTotalMeals = 0;
            decimal grandTotalOvertime = 0;

            foreach (var proj in activeProjects)
            {
                var timesheets = await _context.DailyTimesheets
                    .Include(t => t.AgencyWorker)
                    .Where(t => t.ConstructionProjectId == proj.Id && t.WorkDate.Date == targetDate.Date)
                    .ToListAsync();

                // Eğer puantaj girilmemişse es geçebiliriz veya sıfır olarak gösterebiliriz
                if (!timesheets.Any()) continue;

                int firmWorkersCount = timesheets.Count(t => (t.IsMorningPresent || t.IsAfternoonPresent) && t.AgencyWorker.WorkerType != "Taşeron Personeli");
                int subcontractorCount = timesheets.Count(t => (t.IsMorningPresent || t.IsAfternoonPresent) && t.AgencyWorker.WorkerType == "Taşeron Personeli");
                int totalMeals = timesheets.Count(t => t.IsMorningPresent || t.IsAfternoonPresent); // Kim geldiyse yemek yer (Basit kural)
                decimal totalOvertime = timesheets.Sum(t => t.OvertimeHours);

                projectSummaries.Add(new {
                    ProjectId = proj.Id,
                    ProjectName = proj.Name,
                    FirmWorkers = firmWorkersCount,
                    Subcontractors = subcontractorCount,
                    TotalMeals = totalMeals,
                    TotalOvertime = totalOvertime
                });

                grandTotalFirmWorkers += firmWorkersCount;
                grandTotalSubcontractors += subcontractorCount;
                grandTotalMeals += totalMeals;
                grandTotalOvertime += totalOvertime;
            }

            ViewBag.TargetDate = targetDate.ToString("yyyy-MM-dd");
            ViewBag.GrandTotalFirm = grandTotalFirmWorkers;
            ViewBag.GrandTotalSub = grandTotalSubcontractors;
            ViewBag.GrandTotalMeals = grandTotalMeals;
            ViewBag.GrandTotalOvertime = grandTotalOvertime;

            return View(projectSummaries);
        }
    }

    public class TimesheetSaveRequest
    {
        public int ProjectId { get; set; }
        public string Date { get; set; }
        public List<TimesheetRecord> Records { get; set; }
    }

    public class TimesheetRecord
    {
        public int WorkerId { get; set; }
        public bool IsMorning { get; set; }
        public bool IsAfternoon { get; set; }
        public decimal Overtime { get; set; }
        public string Phase { get; set; }
    }
}
