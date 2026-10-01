import io
import re

filepath = r'GMK360.Web\Controllers\DailyTimesheetController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

global_summary_action = """
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
"""

content = re.sub(r'public class TimesheetSaveRequest', global_summary_action + '\n    public class TimesheetSaveRequest', content)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Injected GlobalSummary action")
