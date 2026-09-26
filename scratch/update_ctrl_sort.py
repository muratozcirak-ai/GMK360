import re

ctrl_file = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with open(ctrl_file, 'r', encoding='utf-8') as f:
    ctrl_content = f.read()

# Replace sorting of Phase0Docs to include DisplayOrder
old_query = '''ViewBag.Phase0Docs = await _context.ProjectLegalDocuments
                .Include(d => d.SystemTemplate)
                .Where(d => d.ConstructionProjectId == id)
                .OrderBy(d => d.Id)
                .ToListAsync();'''

new_query = '''var rawDocs = await _context.ProjectLegalDocuments
                .Include(d => d.SystemTemplate)
                .Where(d => d.ConstructionProjectId == id)
                .OrderBy(d => d.DisplayOrder).ThenBy(d => d.Id)
                .ToListAsync();
                
            ViewBag.Phase0Docs = rawDocs; // Keep original just in case
            ViewBag.GroupedPhase0Docs = rawDocs.GroupBy(d => d.Stage ?? "Diğer Evraklar").ToList();
'''
ctrl_content = ctrl_content.replace(old_query, new_query)

with open(ctrl_file, 'w', encoding='utf-8-sig') as f:
    f.write(ctrl_content)
