import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\AgendaController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace GetDetails to Include(r => r.Items)
content = content.replace('var record = await _context.AgendaRecords', 'var record = await _context.AgendaRecords.Include(r => r.Items)')

# Also add an API endpoint to save the live meeting notes
api_endpoint = '''
        [HttpPost]
        public async Task<IActionResult> SaveLiveNotes(int itemId, string notes)
        {
            var item = await _context.AgendaItems.FindAsync(itemId);
            if (item == null) return NotFound();
            
            item.LiveMeetingNotes = notes;
            await _context.SaveChangesAsync();
            return Ok();
        }
'''
if 'SaveLiveNotes' not in content:
    content = content.replace('return PartialView("_AgendaDetails", record);', 'return PartialView("_AgendaDetails", record);\r\n        }\r\n' + api_endpoint + '\r\n        //')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
