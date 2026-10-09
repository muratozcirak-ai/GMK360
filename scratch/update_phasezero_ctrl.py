import codecs
import re

path = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Update signature
content = content.replace(
    'public async Task<IActionResult> UpdateDoc(int id, string status, string assignedUserId, string institutionContact, decimal? documentFee, decimal? additionalCost, string originalLocation, Microsoft.AspNetCore.Http.IFormFile uploadedFile)',
    'public async Task<IActionResult> UpdateDoc(int id, string status, string assignedUserId, string institutionContact, decimal? documentFee, decimal? additionalCost, string originalLocation, GMK360.Core.Entities.Construction.BudgetPhaseCategory? linkedPhaseCategory, Microsoft.AspNetCore.Http.IFormFile uploadedFile)'
)

# Update doc assignment
content = content.replace(
    'doc.OriginalLocation = originalLocation;',
    'doc.OriginalLocation = originalLocation;\n            doc.LinkedPhaseCategory = linkedPhaseCategory;'
)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)