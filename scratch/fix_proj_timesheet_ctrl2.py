import codecs
import re

path = 'GMK360.Web/Controllers/DailyTimesheetsController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'public async Task<IActionResult> ProjectTimesheet.*?return View\(timesheets\);\s*\}'

replacement = '''public async Task<IActionResult> ProjectTimesheet(int id, DateTime? date)
        {
            var targetDate = date ?? DateTime.Today;
            var project = await _context.ConstructionProjects.FindAsync(id);
            if (project == null) return NotFound();

            var workers = await _context.AgencyWorkers
                .Where(w => !w.IsDeleted)
                .OrderBy(w => w.FirstName)
                .ToListAsync();

            var existingTimesheets = await _context.Set<DailyTimesheet>()
                .Where(t => t.ProjectId == id && t.WorkDate.Date == targetDate.Date)
                .ToDictionaryAsync(t => t.AgencyWorkerId);

            var projects = await _context.ConstructionProjects
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            ViewBag.Project = project;
            ViewBag.TargetDate = targetDate;
            ViewBag.Workers = workers;
            ViewBag.ExistingTimesheets = existingTimesheets;
            ViewBag.Projects = projects;

            return View();
        }'''

content = re.sub(target, replacement, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)