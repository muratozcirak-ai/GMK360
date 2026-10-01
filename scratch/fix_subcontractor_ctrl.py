import codecs
import re

path = 'GMK360.Web/Controllers/SubcontractorContractController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Fix ViewBag for Create GET
content = re.sub(
    r'ViewBag\.PhonebookContacts = await _context\.B2BNetworkContacts[^;]+;',
    '''ViewBag.Subcontractors = await _context.AgencyPhonebooks
                .Where(s => s.AgencyId == agencyId && !s.IsDeleted)
                .ToListAsync();''',
    content
)

# Wait, let's make sure we fix all occurrences.
with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print("Fixed Create GET in Controller")