import codecs
import re

path = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# I will find the end of UpdateDoc method which is:
# return RedirectToAction("Index", new { projectId = doc.ConstructionProjectId });
#         }
update_doc_end = r'return RedirectToAction\("Index", new \{ projectId = doc\.ConstructionProjectId \}\);\s*\}'
match = re.search(update_doc_end, content)

if match:
    # Now find the start of RequestQuote
    request_quote_start = r'public async Task<IActionResult> RequestQuote\(int documentId\)'
    match2 = re.search(request_quote_start, content[match.end():])
    
    if match2:
        part1 = content[:match.end()]
        part3 = content[match.end() + match2.start():]
        
        middle = '''
        
        [HttpPost]
        public async Task<IActionResult> AcceptQuote(int documentId, int inviteId)
        {
            var document = await _context.ProjectLegalDocuments.FindAsync(documentId);
            if (document == null) return NotFound();

            var invite = await _context.B2BQuoteInvites.FindAsync(inviteId);
            if (invite == null) return NotFound();

            invite.Status = GMK360.Core.Entities.B2B.QuoteInviteStatus.Accepted;
            document.EstimatedCost = invite.OfferedPrice;
            document.Status = "İşlemde";
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Teklif başarıyla seçildi ve fizibilite güncellendi.";
            return RedirectToAction(nameof(Index), new { projectId = document.ConstructionProjectId });
        }

        [HttpPost("PhaseZero/RequestQuote/{documentId}")]
        [IgnoreAntiforgeryToken]
        '''
        
        cleaned = part1 + middle + part3
        with codecs.open(path, 'w', 'utf-8-sig') as f:
            f.write(cleaned)