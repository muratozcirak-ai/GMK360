import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs'
with codecs.open(filepath, 'r', 'utf-8') as f:
    content = f.read()

# Details action
details_inject = '''ViewBag.Phase0Docs = await _context.ProjectLegalDocuments
                .Include(d => d.SystemTemplate)
                .Where(d => d.ConstructionProjectId == id)
                .ToListAsync();'''

details_replacement = '''ViewBag.Phase0Docs = await _context.ProjectLegalDocuments
                .Include(d => d.SystemTemplate)
                .Where(d => d.ConstructionProjectId == id)
                .ToListAsync();
            
            var pName = await _context.ConstructionProjects.Where(p => p.Id == id).Select(p => p.Name).FirstOrDefaultAsync();
            ViewData["ProjectName"] = pName;
            ViewData["ProjectId"] = id;'''

content = content.replace(details_inject, details_replacement)

# ManagePhases action
manage_phases_inject = '''var project = await _context.ConstructionProjects'''
manage_phases_replacement = '''ViewData["ProjectName"] = await _context.ConstructionProjects.Where(p => p.Id == id).Select(p => p.Name).FirstOrDefaultAsync();
            ViewData["ProjectId"] = id;
            var project = await _context.ConstructionProjects'''

content = content.replace(manage_phases_inject, manage_phases_replacement)

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
