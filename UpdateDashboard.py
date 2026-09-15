import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\DashboardController.cs'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

pattern = r'(ViewBag\.TeklifProjeler = teklifProjeler;.*?return View\(\);)'

replacement = r'''\1'''

def inject_upcoming(match):
    return '''ViewBag.TeklifProjeler = teklifProjeler;
            
            // Yaklaşan Ödemeler (Gelecek 15 Gün içinde vadesi dolan Çek ve Kredi Kartı ödemeleri)
            var in15Days = DateTime.UtcNow.AddDays(15);
            var upcomingPayments = await _context.Set<GMK360.Core.Entities.Finance.SupplierPayment>()
                .Include(p => p.SupplierCurrentAccount)
                    .ThenInclude(c => c.PhonebookContact)
                .Where(p => p.SupplierCurrentAccount.AgencyId == agency.Id 
                       && p.DueDate.HasValue 
                       && p.DueDate.Value <= in15Days 
                       && p.Status == GMK360.Core.Entities.Finance.PaymentStatus.Pending)
                .OrderBy(p => p.DueDate)
                .Take(5)
                .ToListAsync();
            
            ViewBag.UpcomingPayments = upcomingPayments;

            return View();'''

content = re.sub(pattern, inject_upcoming, content, flags=re.DOTALL)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
