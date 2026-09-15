import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\SubcontractorContractController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

replacement = '''        [HttpGet]
        public async Task<IActionResult> PrintContract(int contractId, int templateId)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var contract = await _context.SubcontractorContracts
                .Include(c => c.Project)
                .Include(c => c.PhonebookContact)
                .FirstOrDefaultAsync(c => c.Id == contractId && c.AgencyId == agencyId.Value);
            
            if (contract == null) return NotFound("Sözleşme bulunamadı.");

            var template = await _context.DocumentTemplates
                .FirstOrDefaultAsync(t => t.Id == templateId && t.AgencyId == agencyId.Value);

            if (template == null) return NotFound("Şablon bulunamadı.");'''

pattern = r'\[HttpGet\]\s*public async Task<IActionResult> PrintContract\(int contractId, int templateId\)\s*\{\s*var agencyId = GetCurrentAgencyId\(\);\s*var contract = await _context\.SubcontractorContracts\s*\.Include\(c => c\.Project\)\s*\.Include\(c => c\.PhonebookContact\)\s*\.FirstOrDefaultAsync\(c => c\.Id == contractId && c\.AgencyId == agencyId\);\s*if \(contract == null\) return NotFound\("Sözleşme bulunamadı\."\);\s*var template = await _context\.DocumentTemplates\s*\.FirstOrDefaultAsync\(t => t\.Id == templateId && t\.AgencyId == agencyId\);\s*if \(template == null\) return NotFound\("Şablon bulunamadı\."\);'

content = re.sub(pattern, replacement, content, flags=re.DOTALL)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
