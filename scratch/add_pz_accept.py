import codecs
import re

path = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

action = '''
        [HttpPost]
        public async Task<IActionResult> AcceptQuote(int documentId, int inviteId)
        {
            var document = await _context.ProjectLegalDocuments.FindAsync(documentId);
            if (document == null) return NotFound();

            var invite = await _context.B2BQuoteInvites.FindAsync(inviteId);
            if (invite == null) return NotFound();

            // Set invite as accepted
            invite.Status = GMK360.Core.Entities.B2B.QuoteInviteStatus.Accepted;
            
            // Set Document Cost (Feasibility)
            document.EstimatedCost = invite.OfferedPrice;
            document.Status = "İşlemde"; // Seçildiğinde işlemde olur
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Teklif başarıyla seçildi ve fizibilite güncellendi.";
            return RedirectToAction(nameof(Index), new { projectId = document.ConstructionProjectId });
        }
'''

if 'public async Task<IActionResult> AcceptQuote' not in content:
    content = content.replace('public async Task<IActionResult> RequestQuote(', action + '\n        public async Task<IActionResult> RequestQuote(')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)