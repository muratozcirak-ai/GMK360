import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\AgendaController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

save_notes = """
        [HttpPost]
        public async Task<IActionResult> SaveLiveNotes(int itemId, string notes)
        {
            var item = await _context.AgendaItems.FindAsync(itemId);
            if (item == null) return NotFound();
            
            item.LiveMeetingNotes = notes;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
"""

content = content.replace('    }\r\n}', save_notes)
content = content.replace('    }\n}', save_notes)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
