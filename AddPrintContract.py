import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\SubcontractorContractController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

new_methods = '''
        [HttpGet]
        public async Task<IActionResult> PrintContract(int contractId, int templateId)
        {
            var agencyId = GetCurrentAgencyId();
            var contract = await _context.SubcontractorContracts
                .Include(c => c.Project)
                .Include(c => c.Subcontractor)
                .FirstOrDefaultAsync(c => c.Id == contractId && c.Project.AgencyId == agencyId);
            
            if (contract == null) return NotFound("Sözleşme bulunamadı.");

            var template = await _context.DocumentTemplates
                .FirstOrDefaultAsync(t => t.Id == templateId && t.AgencyId == agencyId);

            if (template == null) return NotFound("Şablon bulunamadı.");

            // Replace variables
            string finalHtml = template.HtmlContent ?? "";
            
            string companyName = contract.Subcontractor?.FullNameOrCompanyName ?? "_______________";
            string projectName = contract.Project?.Name ?? "_______________";
            string totalAmount = contract.TotalAmount.ToString("N2") + " " + contract.Currency;
            string jobDesc = contract.JobDescription ?? "_______________";
            string contractDate = contract.ContractDate.ToString("dd.MM.yyyy");

            finalHtml = finalHtml.Replace("{{FirmaAdi}}", companyName)
                                 .Replace("{{ProjeAdi}}", projectName)
                                 .Replace("{{Tutar}}", totalAmount)
                                 .Replace("{{IsTanimi}}", jobDesc)
                                 .Replace("{{Tarih}}", contractDate);

            ViewBag.PrintContent = finalHtml;
            ViewBag.Title = $"{companyName} - {template.TemplateName}";
            
            return View("PrintPreview", contract);
        }
'''

if 'public async Task<IActionResult> PrintContract' not in content:
    # Insert before the last closing brace of the controller class
    last_brace_index = content.rfind('}')
    if last_brace_index > 0:
        last_brace_index = content.rfind('}', 0, last_brace_index - 1)
        
    content = content[:last_brace_index] + new_methods + "\n" + content[last_brace_index:]

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
