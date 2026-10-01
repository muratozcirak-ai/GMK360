import codecs
path = 'GMK360.Web/Controllers/InventoryController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace the condition with just !p.IsDeleted
import re
content = re.sub(r'var projects = await _context\.ConstructionProjects\s*\.Where\(p => p\.AgencyId == agencyId\.Value && !p\.IsDeleted[^)]+\)\s*\.ToListAsync\(\);', 'var projects = await _context.ConstructionProjects\n                    .Where(p => p.AgencyId == agencyId.Value && !p.IsDeleted)\n                    .ToListAsync();', content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)