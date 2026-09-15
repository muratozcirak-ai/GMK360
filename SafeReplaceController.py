import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\SubcontractorContractController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace Details logic
details_pattern = r'var payments = await _context\.ProgressPayments.*?\n.*?ViewBag\.TotalApproved = payments\.Where.*?\.Sum\(p => p\.ApprovedAmount\);'
details_repl = '''var hakedisler = await _context.SubcontractorHakedisler
                .Where(h => h.ContractId == id)
                .OrderByDescending(h => h.HakedisNo)
                .ToListAsync();

            ViewBag.Hakedisler = hakedisler;
            var totalClaim = hakedisler.Sum(h => h.ClaimAmount);
            var totalDeduction = hakedisler.Sum(h => h.DeductionAmount);
            ViewBag.TotalPaid = totalClaim - totalDeduction;
            ViewBag.TotalApproved = totalClaim - totalDeduction;

            ViewBag.Templates = await _context.DocumentTemplates.Where(t => t.AgencyId == agencyId && !t.IsDeleted).ToListAsync();'''
content = re.sub(details_pattern, details_repl, content, flags=re.DOTALL)

# Remove AddPayment and ApprovePayment by just replacing their bodies with comments
# We can't easily regex the full body due to curly braces, so let's match the signature and just remove up to a specific string.
# Wait, ApprovePayment ends with eturn RedirectToAction(nameof(Details), new { id = payment.SubcontractorContractId });\n        }
add_approve_pattern = r'\[HttpPost\]\s*public async Task<IActionResult> AddPayment.*?return RedirectToAction\(nameof\(Details\), new \{ id = payment\.SubcontractorContractId \}\);\s*\}'
add_hakedis_repl = '''
        [HttpPost]
        public async Task<IActionResult> AddHakedis(int contractId, decimal claimAmount, decimal deductionAmount, string deductionReason, string description)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var contract = await _context.SubcontractorContracts
                .Include(c => c.Hakedisler)
                .FirstOrDefaultAsync(c => c.Id == contractId && c.AgencyId == agencyId.Value);

            if (contract == null) return NotFound();

            var pastClaimsTotal = contract.Hakedisler.Sum(h => h.ClaimAmount);
            if (pastClaimsTotal + claimAmount > contract.TotalAmount)
            {
                TempData["ErrorMessage"] = $""Dikkat: Bu hakediş ile sözleşme bedeli (%100) aşılmaktadır! Maksimum eklenebilir brüt tutar: {(contract.TotalAmount - pastClaimsTotal).ToString("N2")}"";
                return RedirectToAction(""Index"");
            }

            var nextNo = contract.Hakedisler.Any() ? contract.Hakedisler.Max(h => h.HakedisNo) + 1 : 1;

            var hakedis = new SubcontractorHakedis
            {
                ContractId = contract.Id,
                HakedisNo = nextNo,
                HakedisDate = DateTime.Now,
                Description = description,
                ClaimAmount = claimAmount,
                DeductionAmount = deductionAmount,
                DeductionReason = deductionReason,
                IsApproved = true // Otomatik onaylı varsayıyoruz
            };

            _context.SubcontractorHakedisler.Add(hakedis);
            
            // Cari hesaba yansit
            var contactId = contract.PhonebookContactId;
            var currentAccount = await _context.SupplierCurrentAccounts
                .FirstOrDefaultAsync(a => a.AgencyId == agencyId && a.PhonebookContactId == contactId);

            if (currentAccount == null)
            {
                currentAccount = new SupplierCurrentAccount
                {
                    AgencyId = agencyId.Value,
                    PhonebookContactId = contactId,
                    CurrentBalance = 0
                };
                _context.SupplierCurrentAccounts.Add(currentAccount);
                await _context.SaveChangesAsync();
            }

            decimal net = claimAmount - deductionAmount;
            currentAccount.CurrentBalance += net;

            var transaction = new SupplierAccountTransaction
            {
                SupplierCurrentAccountId = currentAccount.Id,
                TransactionDate = DateTime.Now,
                Type = SupplierTransactionType.PurchaseInvoice, 
                Amount = net,
                BalanceAfterTransaction = currentAccount.CurrentBalance,
                Description = $""{nextNo}. Hakediş: {contract.Title}"",
                CreatedByUserId = _userManager.GetUserId(User) ?? """"
            };
            
            _context.SupplierAccountTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $""{nextNo}. Hakediş başarıyla eklendi."";
            return RedirectToAction(""Index"");
        }'''
content = re.sub(add_approve_pattern, add_hakedis_repl, content, flags=re.DOTALL)

# Add PrintContract just before the last }
print_contract_code = '''
        [HttpGet]
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
        }
'''

content = content.replace('ModelState.Remove("ProgressPayments");', 'ModelState.Remove("Hakedisler");')
content = content.replace('using GMK360.Core.Entities.Finance;', 'using GMK360.Core.Entities.Finance;\nusing GMK360.Core.Entities.Construction;')

last_brace_index = content.rfind('}')
if last_brace_index > 0:
    last_brace_index = content.rfind('}', 0, last_brace_index - 1)
content = content[:last_brace_index] + print_contract_code + "\n" + content[last_brace_index:]

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
