import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\DailyTimesheetsController.cs'

with codecs.open(filepath, 'r', 'cp1254') as f:
    content = f.read()

pattern = r'public async Task<IActionResult> SaveTimesheets.*?return RedirectToAction.*?}'

def replacement(match):
    return '''[HttpPost]
        public async Task<IActionResult> SaveTimesheets(int? sourceProjectId, DateTime targetDate, int[] workerIds, int[] phaseIds, string[] statuses, string[] notesList)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            if (workerIds == null || workerIds.Length == 0)
                return RedirectToAction(nameof(ProjectTimesheet), new { id = sourceProjectId, date = targetDate.ToString("yyyy-MM-dd") });

            // Fetch workers to know their wages
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
        }'''

content = re.sub(pattern, replacement, content, flags=re.DOTALL)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
