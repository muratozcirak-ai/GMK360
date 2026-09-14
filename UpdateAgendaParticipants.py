import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\AgendaController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace GetCreate
get_create = '''        public IActionResult Create()
        {
            var agencyId = GetCurrentAgencyId();
            ViewBag.ProjectId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.ConstructionProjects.Where(p => p.AgencyId == agencyId), "Id", "Name");
            ViewBag.PhonebookId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.AgencyPhonebooks.Where(p => p.AgencyId == agencyId), "Id", "Name");
            // Also multiselect for participants
            ViewBag.ParticipantsList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.AgencyPhonebooks.Where(p => p.AgencyId == agencyId), "Id", "Name");
            return View(new AgendaRecord { EventDate = DateTime.Now });
        }'''

content = content.replace('''        public IActionResult Create()
        {
            var agencyId = GetCurrentAgencyId();
            ViewBag.ProjectId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.ConstructionProjects.Where(p => p.AgencyId == agencyId), "Id", "Name");
            ViewBag.PhonebookId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.AgencyPhonebooks.Where(p => p.AgencyId == agencyId), "Id", "Name");
            return View(new AgendaRecord { EventDate = DateTime.Now });
        }''', get_create)

# Modify POST Create definition
post_create_sig_old = 'public async Task<IActionResult> Create(AgendaRecord record, IFormFile imageFile, List<string> itemTopicTitle, List<string> itemPresentationText)'
post_create_sig_new = 'public async Task<IActionResult> Create(AgendaRecord record, IFormFile imageFile, List<string> itemTopicTitle, List<string> itemPresentationText, List<int> selectedParticipants)'
content = content.replace(post_create_sig_old, post_create_sig_new)

# Add logic for selectedParticipants
participants_logic = '''
            // Katılımcılar
            record.Participants = new System.Collections.Generic.List<AgendaParticipant>();
            if (selectedParticipants != null && selectedParticipants.Count > 0)
            {
                foreach(var pId in selectedParticipants)
                {
                    var phonebook = _context.AgencyPhonebooks.FirstOrDefault(p => p.Id == pId);
                    if (phonebook != null)
                    {
                        var participant = new AgendaParticipant { PhonebookId = pId };
                        if (!string.IsNullOrEmpty(phonebook.LinkedUserId))
                        {
                            participant.UserId = phonebook.LinkedUserId;
                        }
                        else
                        {
                            participant.AccessToken = Guid.NewGuid().ToString("N");
                        }
                        record.Participants.Add(participant);
                    }
                }
            }
            
            _context.AgendaRecords.Add(record);
'''
content = content.replace('_context.AgendaRecords.Add(record);', participants_logic)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
