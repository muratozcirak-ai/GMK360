import codecs
import re

path = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('additionalCost = doc.AdditionalCost ?? 0,', 'additionalCost = doc.AdditionalCost ?? 0,\n                originalLocation = doc.OriginalLocation,')

content = content.replace('public async Task<IActionResult> UpdateDoc(int id, string status, string assignedUserId, string institutionContact, decimal documentFee, decimal additionalCost, Microsoft.AspNetCore.Http.IFormFile uploadedFile)', 'public async Task<IActionResult> UpdateDoc(int id, string status, string assignedUserId, string institutionContact, decimal documentFee, decimal additionalCost, string originalLocation, Microsoft.AspNetCore.Http.IFormFile uploadedFile)')

content = content.replace('doc.AdditionalCost = additionalCost;', 'doc.AdditionalCost = additionalCost;\n            doc.OriginalLocation = originalLocation;')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)