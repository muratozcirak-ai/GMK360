import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\AgendaController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

get_create_method = """
        public IActionResult Create()
        {
            var agencyId = GetCurrentAgencyId();
            ViewBag.ProjectId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.ConstructionProjects.Where(p => p.AgencyId == agencyId), "Id", "Name");
            ViewBag.PhonebookId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.AgencyPhonebooks.Where(p => p.AgencyId == agencyId), "Id", "Name");
            return View(new AgendaRecord { EventDate = DateTime.Now });
        }

        [HttpPost]"""

content = content.replace('[HttpPost]', get_create_method, 1)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
