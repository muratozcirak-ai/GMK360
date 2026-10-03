import codecs
import re

path = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Modify RequestQuote to redirect to B2BPurchasing/Details
target = r'_context\.B2BQuoteRequests\.Add\(quoteRequest\);\s*doc\.Status = "Fiyat Araştırılıyor";\s*doc\.AssignedUserId = "Satınalma Departmanı";\s*await _context\.SaveChangesAsync\(\);\s*\}\s*return RedirectToAction\("Index", new \{ projectId = doc\.ConstructionProjectId \}\);'
replacement = '''_context.B2BQuoteRequests.Add(quoteRequest);
                doc.Status = "Fiyat Araştırılıyor";
                doc.AssignedUserId = "Satınalma Departmanı";
                await _context.SaveChangesAsync();
            } else {
                var existingQuote = await _context.B2BQuoteRequests.FirstOrDefaultAsync(q => q.SourceModule == "PhaseZeroDocument" && q.SourceReferenceId == documentId);
                if (existingQuote != null) {
                    return RedirectToAction("Details", "B2BPurchasing", new { id = existingQuote.Id });
                }
            }

            var newQuote = await _context.B2BQuoteRequests.FirstOrDefaultAsync(q => q.SourceModule == "PhaseZeroDocument" && q.SourceReferenceId == documentId);
            if (newQuote != null) {
                return RedirectToAction("Details", "B2BPurchasing", new { id = newQuote.Id });
            }
            return RedirectToAction("Index", new { projectId = doc.ConstructionProjectId });'''

content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)