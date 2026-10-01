import codecs

path = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

replacement = '''
            ViewBag.Phase0Docs = await _context.ProjectLegalDocuments
                .Include(d => d.SystemTemplate)
                .Where(d => d.ConstructionProjectId == id)
                .ToListAsync();

            // KURAL: Paydaşları View'a gönder
            ViewBag.Stakeholders = await _context.ProjectStakeholders
                .Include(s => s.User)
                .Where(s => s.ProjectId == id && !s.IsDeleted)
                .ToListAsync();
'''

# Find the exact code to replace
old_code = '''
            ViewBag.Phase0Docs = await _context.ProjectLegalDocuments
                .Include(d => d.SystemTemplate)
                .Where(d => d.ConstructionProjectId == id)
                .ToListAsync();'''
                
if 'ViewBag.Stakeholders = await' not in content:
    content = content.replace(old_code, replacement)
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)
    print('Injected ViewBag.Stakeholders successfully.')
else:
    print('ViewBag.Stakeholders already exists.')