import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\SubcontractorContractController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

replacement = '''
            ViewBag.Payments = await _context.ProgressPayments
                .Where(p => p.ContractId == id)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            ViewBag.TotalPaid = ViewBag.Payments.Sum(p => p.PaidAmount);
            ViewBag.TotalApproved = ViewBag.Payments.Sum(p => p.ApprovedAmount);
            
            ViewBag.Templates = await _context.DocumentTemplates.Where(t => t.AgencyId == agencyId && !t.IsDeleted).ToListAsync();
'''

pattern = r'ViewBag\.Payments = await _context\.ProgressPayments.*?\n.*?ViewBag\.TotalApproved = ViewBag\.Payments\.Sum\(p => p\.ApprovedAmount\);'
content = re.sub(pattern, replacement, content, flags=re.DOTALL)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
