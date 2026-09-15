import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\SubcontractorContractController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace Details logic
pattern_details = r'ViewBag\.Payments = await _context\.ProgressPayments.*?\n.*?ViewBag\.TotalApproved = ViewBag\.Payments\.Sum\(p => p\.ApprovedAmount\);'
replacement_details = '''
            ViewBag.Hakedisler = await _context.SubcontractorHakedisler
                .Where(h => h.ContractId == id)
                .OrderByDescending(h => h.HakedisNo)
                .ToListAsync();

            var totalClaim = ((List<SubcontractorHakedis>)ViewBag.Hakedisler).Sum(h => h.ClaimAmount);
            var totalDeduction = ((List<SubcontractorHakedis>)ViewBag.Hakedisler).Sum(h => h.DeductionAmount);
            ViewBag.TotalPaid = totalClaim - totalDeduction;
            ViewBag.TotalApproved = totalClaim - totalDeduction;
'''
content = re.sub(pattern_details, replacement_details, content, flags=re.DOTALL)

# Delete existing AddPayment and ApprovePayment methods
content = re.sub(r'\[HttpPost\]\s*public async Task<IActionResult> AddPayment.*?\n\s*\}\s*', '', content, flags=re.DOTALL)
content = re.sub(r'\[HttpPost\]\s*public async Task<IActionResult> ApprovePayment.*?\n\s*\}\s*', '', content, flags=re.DOTALL)

# Add new AddHakedis method
new_methods = '''
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
                TempData["ErrorMessage"] = $"Dikkat: Bu hakediş ile sözleşme bedeli (%100) aşılmaktadır! Maksimum eklenebilir brüt tutar: {(contract.TotalAmount - pastClaimsTotal).ToString("N2")}";
                return RedirectToAction("Index", new { projectId = contract.ProjectId });
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
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{nextNo}. Hakediş başarıyla eklendi.";
            return RedirectToAction("Index", new { projectId = contract.ProjectId });
        }
'''

last_brace_index = content.rfind('}')
if last_brace_index > 0:
    last_brace_index = content.rfind('}', 0, last_brace_index - 1)
content = content[:last_brace_index] + new_methods + "\n" + content[last_brace_index:]

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
