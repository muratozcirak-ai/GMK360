import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\AgendaController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

shadow_user_method = '''
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var participant = await _context.AgendaParticipants
                .Include(p => p.AgendaRecord)
                .ThenInclude(r => r.Items)
                .FirstOrDefaultAsync(p => p.AgendaRecordId == id && p.UserId == userId);

            if (participant == null) return NotFound("Bu toplantıya katılma yetkiniz yok veya böyle bir toplantı bulunamadı.");

            if (!participant.IsViewed)
            {
                participant.IsViewed = true;
                await _context.SaveChangesAsync();
            }

            ViewBag.ParticipantId = participant.Id;
            ViewBag.ParticipantNotes = participant.ParticipantNotes;
            return View("GuestView", participant.AgendaRecord);
        }
'''

content = content.replace('    }\r\n}', shadow_user_method + '\r\n    }\r\n}')
content = content.replace('    }\n}', shadow_user_method + '\n    }\n}')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
