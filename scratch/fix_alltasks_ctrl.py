import codecs
import re

path = 'GMK360.Web/Controllers/CompanyGarageController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'public async Task<IActionResult> AllTasks\(string\? date\)\s*\{'
replacement = '''public async Task<IActionResult> AllTasks(string? date)
        {
            ViewBag.Projects = await _context.ConstructionProjects.Where(p => !p.IsDeleted).Select(p => new { p.Id, p.Name }).ToListAsync();'''
content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)