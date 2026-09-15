import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\SubcontractorContractController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('ViewBag.Templates = await _context.DocumentTemplates.Where(t => t.AgencyId == agencyId && !t.IsDeleted).ToListAsync();', 'ViewBag.Templates = await _context.DocumentTemplates.Where(t => t.AgencyId == agencyId.Value && !t.IsDeleted).ToListAsync();')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
