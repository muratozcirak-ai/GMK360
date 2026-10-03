import codecs
import re

path = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'\[HttpPost\("PhaseZero/UpdateDoc"\)\]\s*public async Task<IActionResult> UpdateDoc\(int id, string status, string assignedUserId, string institutionContact, decimal documentFee, decimal additionalCost, string originalLocation, Microsoft\.AspNetCore\.Http\.IFormFile uploadedFile\)'

replacement = '''[HttpPost("PhaseZero/UpdateDoc")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateDoc(int id, string status, string assignedUserId, string institutionContact, decimal? documentFee, decimal? additionalCost, string originalLocation, Microsoft.AspNetCore.Http.IFormFile uploadedFile)'''

content = re.sub(target, replacement, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)