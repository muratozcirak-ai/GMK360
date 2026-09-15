import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\DashboardController.cs'
try:
    with codecs.open(filepath, 'r', 'utf-8-sig') as f:
        content = f.read()
except UnicodeDecodeError:
    with codecs.open(filepath, 'r', 'cp1254') as f:
        content = f.read()

replacement = '''var totalPaymentsThisMonth = await _context.SubcontractorHakedisler
                .Include(p => p.Contract)
                .Where(p => p.Contract.AgencyId == agency.Id && p.IsApproved == true && p.HakedisDate >= firstDayOfMonth)
                .SumAsync(p => p.ClaimAmount - p.DeductionAmount);'''

content = re.sub(r'var totalPaymentsThisMonth = await _context\.ProgressPayments.*?SumAsync\(p => p\.ApprovedAmount\);', replacement, content, flags=re.DOTALL)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
