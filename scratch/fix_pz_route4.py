import codecs
import re

path = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# I will find every instance of [HttpPost] public async Task<IActionResult> AcceptQuote
# and remove it up to its closing brace.
# Since it might be nested or have different spaces, I'll use a strong regex.
# Actually, it's easier to just split by 'public async Task<IActionResult> AcceptQuote' and rebuild.

blocks = re.split(r'\[HttpPost\]\s*public async Task<IActionResult> AcceptQuote\(int documentId, int inviteId\)\s*\{', content)

# blocks[0] is everything before the first one.
# For blocks[1:], we need to find the matching closing brace.
cleaned_content = blocks[0]

for block in blocks[1:]:
    # find the first '}' that is at the same indentation level?
    # Or just find 'return RedirectToAction' and the next '}'
    match = re.search(r'return RedirectToAction\(nameof\(Index\), new \{ projectId = document\.ConstructionProjectId \} \);\s*\}', block)
    if match:
        cleaned_content += block[match.end():]
    else:
        # fallback if not exactly matched
        cleaned_content += block

# Also remove any stray [IgnoreAntiforgeryToken] that might be floating before RequestQuote
cleaned_content = re.sub(r'\[IgnoreAntiforgeryToken\]\s*\[IgnoreAntiforgeryToken\]', '[IgnoreAntiforgeryToken]', cleaned_content)

# Now, add it exactly once before RequestQuote
target = r'(\[HttpPost\("PhaseZero/RequestQuote/\{documentId\}"\)\]\s*\[IgnoreAntiforgeryToken\]\s*public async Task<IActionResult> RequestQuote)'
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

        \\1'''

cleaned_content = re.sub(target, replacement, cleaned_content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(cleaned_content)