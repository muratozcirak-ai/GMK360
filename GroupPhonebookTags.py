import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\AgendaController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

get_create_old = '''        public IActionResult Create()
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

get_create_new = '''        public IActionResult Create()
        {
            var agencyId = GetCurrentAgencyId();
            ViewBag.ProjectId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.ConstructionProjects.Where(p => p.AgencyId == agencyId), "Id", "Name");
            
            var phonebooks = _context.AgencyPhonebooks
                .Where(p => p.AgencyId == agencyId)
                .Select(p => new {
                    Id = p.Id,
                    // Eğer Tags alanı doluysa, parantez içinde sonuna ekliyoruz ki Select2 aramasında (örn: Betoncu, Kalıpçı) çıksın.
                    Name = string.IsNullOrEmpty(p.Tags) ? p.Name : p.Name + " [" + p.Tags + "]",
                    GroupName = p.ContactType == 1 ? "Ustalar / Ekipler" :
                                p.ContactType == 2 ? "Taşeron Firmalar" :
                                p.ContactType == 3 ? "Tedarikçiler" : "Müşavir / Ofis / Diğer"
                })
                .OrderBy(p => p.GroupName)
                .ThenBy(p => p.Name)
                .ToList();

            ViewBag.PhonebookId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(phonebooks, "Id", "Name", null, "GroupName");
            ViewBag.ParticipantsList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(phonebooks, "Id", "Name", null, "GroupName");
            
            return View(new AgendaRecord { EventDate = DateTime.Now });
        }'''

if get_create_old in content:
    content = content.replace(get_create_old, get_create_new)
else:
    print("Not found!")

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
