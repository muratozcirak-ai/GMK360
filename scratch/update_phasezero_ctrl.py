import io
import re

# UPDATE CONTROLLER
filepath = r'GMK360.Web\Controllers\PhaseZeroController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

injection = """            ViewBag.GlobalRules = await _context.ModuleDocumentRules
                .Include(r => r.Prerequisites)
                .ThenInclude(p => p.PrerequisiteTemplate)
                .Where(r => r.TargetModule == "Construction")
                .ToListAsync();

            // Fetch quote requests for these documents to show status
            var docIds = docs.Select(d => d.Id).ToList();
            var quoteRequests = await _context.B2BQuoteRequests
                .Include(q => q.Invites)
                .Where(q => q.SourceModule == "PhaseZeroDocument" && docIds.Contains(q.SourceReferenceId))
                .ToListAsync();
            
            ViewBag.Quotes = quoteRequests;
"""

content = content.replace("""            ViewBag.GlobalRules = await _context.ModuleDocumentRules
                .Include(r => r.Prerequisites)
                .ThenInclude(p => p.PrerequisiteTemplate)
                .Where(r => r.TargetModule == "Construction")
                .ToListAsync();""", injection)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Updated PhaseZeroController")
