import io
import re

filepath = r'GMK360.Web\Controllers\PhaseZeroController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Add RequestQuote endpoint
new_method = """
        [HttpPost("PhaseZero/RequestQuote/{documentId}")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RequestQuote(int documentId)
        {
            var doc = await _context.ProjectLegalDocuments
                .Include(d => d.ConstructionProject)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (doc == null) return NotFound();

            // Sadece daha önce aynı evrak için talep açılmamışsa aç
            bool exists = await _context.B2BQuoteRequests.AnyAsync(q => q.SourceModule == "PhaseZeroDocument" && q.SourceReferenceId == documentId);
            
            if (!exists)
            {
                var quoteRequest = new GMK360.Core.Entities.B2B.B2BQuoteRequest
                {
                    RequesterAgencyId = doc.ConstructionProject.AgencyId ?? 1,
                    RequesterUserId = "ProjeYöneticisi",
                    SourceModule = "PhaseZeroDocument",
                    SourceReferenceId = doc.Id,
                    Title = doc.ConstructionProject.Name + " - " + doc.DocumentName + " (Fiyat Araştırması)",
                    Description = "Bu proje (" + doc.ConstructionProject.Name + ") için bu evrak/işlem (" + doc.DocumentName + ") lazım. Fiyat araştırması ve onayını bekliyorum.",
                    Deadline = System.DateTime.Now.AddDays(7),
                    Status = "Draft"
                };

                _context.B2BQuoteRequests.Add(quoteRequest);
                
                doc.Status = "İşlemde"; // Satınalma sürecine girdi
                doc.AssignedUserId = "Satınalma Departmanı"; // Otomatik satınalmaya atandı
                
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { projectId = doc.ConstructionProjectId });
        }
"""

# Inject before the last closing brace (assuming it's the class/namespace end)
# Find the last closing brace of the class PhaseZeroController
last_brace_index = content.rfind("}")
if last_brace_index != -1:
    class_close_index = content.rfind("}", 0, last_brace_index)
    if class_close_index != -1:
        content = content[:class_close_index] + new_method + content[class_close_index:]

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Added RequestQuote method successfully.")
