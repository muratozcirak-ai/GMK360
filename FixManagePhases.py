import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs'
with codecs.open(filepath, 'r', 'utf-8', errors='ignore') as f:
    content = f.read()

# ManagePhases(int? id)
managephases_find = '''public async Task<IActionResult> ManagePhases(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.ConstructionProjects'''

managephases_replace = '''public async Task<IActionResult> ManagePhases(int? id)
        {
            if (id == null) return NotFound();

            ViewData["ProjectName"] = await _context.ConstructionProjects.Where(p => p.Id == id).Select(p => p.Name).FirstOrDefaultAsync();
            ViewData["ProjectId"] = id;

            var project = await _context.ConstructionProjects'''
            
content = content.replace(managephases_find, managephases_replace, 1)

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
