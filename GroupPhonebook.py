import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\AgendaController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

get_create_old = '''        public IActionResult Create()
        {
            var agencyId = GetCurrentAgencyId();
            ViewBag.ProjectId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.ConstructionProjects.Where(p => p.AgencyId == agencyId), "Id", "Name");
            ViewBag.PhonebookId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.AgencyPhonebooks.Where(p => p.AgencyId == agencyId), "Id", "Name");
            // Also multiselect for participants
            ViewBag.ParticipantsList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.AgencyPhonebooks.Where(p => p.AgencyId == agencyId), "Id", "Name");
            return View(new AgendaRecord { EventDate = DateTime.Now });
        }'''

get_create_new = '''        public IActionResult Create()
        {
            var agencyId = GetCurrentAgencyId();
            ViewBag.ProjectId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.ConstructionProjects.Where(p => p.AgencyId == agencyId), "Id", "Name");
            
            var phonebooks = _context.AgencyPhonebooks
                .Where(p => p.AgencyId == agencyId)
                .Select(p => new {
                    Id = p.Id,
                    Name = p.Name,
                    GroupName = p.ContactType == 1 ? "Ustalar / Ekipler" :
                                p.ContactType == 2 ? "Taşeron Firmalar" :
                                p.ContactType == 3 ? "Tedarikçiler" : "Ofis / Diğer (Mimar, Mühendis vb.)"
                }).ToList();

            ViewBag.PhonebookId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(phonebooks, "Id", "Name", null, "GroupName");
            ViewBag.ParticipantsList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(phonebooks, "Id", "Name", null, "GroupName");
            
            return View(new AgendaRecord { EventDate = DateTime.Now });
        }'''

if get_create_old in content:
    content = content.replace(get_create_old, get_create_new)
else:
    print("Old Create not found! Fallback to regex...")
    pattern = r'public IActionResult Create\(\)\s*\{.*?return View\(new AgendaRecord \{ EventDate = DateTime.Now \}\);\s*\}'
    content = re.sub(pattern, get_create_new, content, flags=re.DOTALL)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
