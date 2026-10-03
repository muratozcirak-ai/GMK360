import codecs
import re

path = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# First, remove ALL AcceptQuote definitions completely.
content = re.sub(r'\s*\[HttpPost\]\s*public async Task<IActionResult> AcceptQuote\(int documentId, int inviteId\)\s*\{.*?\s*return RedirectToAction\(nameof\(Index\), new \{ projectId = document\.ConstructionProjectId \} \);\s*\}', '', content, flags=re.DOTALL)
content = re.sub(r'\s*\[IgnoreAntiforgeryToken\]', '', content, flags=re.DOTALL)

# Now, cleanly insert it before RequestQuote
target = r'(\[HttpPost\("PhaseZero/RequestQuote/\{documentId\}"\)\])'
replacement = '''[HttpPost]
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

        [IgnoreAntiforgeryToken]
        \\1'''

content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)