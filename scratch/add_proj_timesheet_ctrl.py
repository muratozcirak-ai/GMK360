import codecs
import re

path = 'GMK360.Web/Controllers/DailyTimesheetsController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

action = '''
        public async Task<IActionResult> ProjectTimesheet(int id, DateTime? workDate)
        {
            var date = workDate ?? DateTime.Today;
            var project = await _context.ConstructionProjects.FindAsync(id);
            if (project == null) return NotFound();

            ViewBag.CurrentDate = date.ToString("yyyy-MM-dd");
            ViewData["ProjectId"] = project.Id;
            ViewData["ProjectName"] = project.Name;

            var query = _context.Set<DailyTimesheet>()
                .Include(t => t.AgencyWorker)
                .Where(t => t.ProjectId == id && t.WorkDate.Date == date.Date);

            var timesheets = await query.ToListAsync();
            return View(timesheets);
        }
'''

if 'ProjectTimesheet' not in content:
    content = content.replace('public async Task<IActionResult> Index(', action + '\n        public async Task<IActionResult> Index(')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Added ProjectTimesheet action.')