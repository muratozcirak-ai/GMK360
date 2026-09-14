import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\AgendaController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

guest_method = '''
        [AllowAnonymous]
        public async Task<IActionResult> Guest(string token)
        {
            if (string.IsNullOrEmpty(token)) return NotFound();

            var participant = await _context.AgendaParticipants
                .Include(p => p.AgendaRecord)
                .ThenInclude(r => r.Items)
                .FirstOrDefaultAsync(p => p.AccessToken == token);

            if (participant == null) return NotFound();
            
            // Mark as viewed
            if (!participant.IsViewed)
            {
                participant.IsViewed = true;
                await _context.SaveChangesAsync();
            }

            ViewBag.ParticipantId = participant.Id;
            ViewBag.ParticipantNotes = participant.ParticipantNotes;
            return View("GuestView", participant.AgendaRecord);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SaveParticipantNotes(int participantId, string notes)
        {
            var p = await _context.AgendaParticipants.FindAsync(participantId);
            if (p != null)
            {
                p.ParticipantNotes = notes;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
'''

content = content.replace('    }\r\n}', guest_method + '\r\n    }\r\n}')
content = content.replace('    }\n}', guest_method + '\n    }\n}')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
