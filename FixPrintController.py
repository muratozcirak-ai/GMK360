import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\SubcontractorContractController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# I will replace the PrintContract method I just injected with the corrected one.
replacement_method = '''        [HttpGet]
        public async Task<IActionResult> PrintContract(int contractId, int templateId)
        {
            var agencyId = GetCurrentAgencyId();
            var contract = await _context.SubcontractorContracts
                .Include(c => c.Project)
                .Include(c => c.PhonebookContact)
                .FirstOrDefaultAsync(c => c.Id == contractId && c.AgencyId == agencyId);
            
            if (contract == null) return NotFound("Sözleşme bulunamadı.");

            var template = await _context.DocumentTemplates
                .FirstOrDefaultAsync(t => t.Id == templateId && t.AgencyId == agencyId);

            if (template == null) return NotFound("Şablon bulunamadı.");

            // Replace variables
            string finalHtml = template.HtmlContent ?? "";
            
            string companyName = contract.PhonebookContact?.Name ?? "_______________";
            string projectName = contract.Project?.Name ?? "_______________";
            string totalAmount = contract.TotalAmount.ToString("N2") + " " + contract.Currency;
            string jobDesc = contract.Description ?? "_______________";
            string contractDate = contract.ContractDate.ToString("dd.MM.yyyy");

            finalHtml = finalHtml.Replace("{{FirmaAdi}}", companyName)
                                 .Replace("{{ProjeAdi}}", projectName)
                                 .Replace("{{Tutar}}", totalAmount)
                                 .Replace("{{IsTanimi}}", jobDesc)
                                 .Replace("{{Tarih}}", contractDate);

            ViewBag.PrintContent = finalHtml;
            ViewBag.Title = $"{companyName} - {template.TemplateName}";
            
            return View("PrintPreview", contract);
        }'''

pattern = r'\[HttpGet\]\s*public async Task<IActionResult> PrintContract\(int contractId, int templateId\)\s*\{.*?return View\("PrintPreview", contract\);\s*\}'
content = re.sub(pattern, replacement_method, content, flags=re.DOTALL)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
