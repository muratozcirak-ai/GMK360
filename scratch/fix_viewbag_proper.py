import codecs

path = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

import re

# We will just find where ViewBag.Phase0Docs is assigned in Details, and inject after it.
match = re.search(r'ViewBag\.Phase0Docs = [^;]+;', content, re.DOTALL)
if match:
    inject = '''
            ViewBag.Stakeholders = await _context.ProjectStakeholders
                .Include(s => s.User)
                .Where(s => s.ProjectId == id && !s.IsDeleted)
                .ToListAsync();
'''
    if 'ViewBag.Stakeholders = await _context.ProjectStakeholders' not in content:
        content = content[:match.end()] + inject + content[match.end():]
        with codecs.open(path, 'w', 'utf-8-sig') as f:
            f.write(content)
        print('Injected successfully!')
    else:
        print('Already exists')
else:
    print('Match not found!')