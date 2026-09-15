import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\PartnerPortalController.cs'

content = '''using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities.Identity;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Finance;
using GMK360.Data.Contexts;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class PartnerPortalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PartnerPortalController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Gölge kullanıcının rehber kaydını bulur
        private async Task<AgencyPhonebook?> GetPartnerContactAsync()
        {
            var userId = _userManager.GetUserId(User);
            // Kendi ID'si LinkedUserId olarak atanmış olan Phonebook kaydını bul.
            return await _context.AgencyPhonebooks.FirstOrDefaultAsync(p => p.LinkedUserId == userId);
        }

        public async Task<IActionResult> Index()
        {
            var contact = await GetPartnerContactAsync();
            if (contact == null) return View("NotAPartner");

            var account = await _context.Set<SupplierCurrentAccount>()
                .Include(a => a.Transactions.OrderByDescending(t => t.TransactionDate).Take(5))
                .Include(a => a.Payments.OrderByDescending(p => p.PaymentDate).Take(5))
                .FirstOrDefaultAsync(a => a.PhonebookContactId == contact.Id);

            ViewBag.Contact = contact;
            ViewBag.Account = account;
            
            // Eğer taşeronsa sözleşmelerini getir
            var contracts = await _context.Set<SubcontractorContract>()
                .Include(c => c.Project)
                .Where(c => c.SubcontractorId == contact.Id)
                .ToListAsync();
            
            ViewBag.Contracts = contracts;

            return View();
        }

        public async Task<IActionResult> Ledger()
        {
            var contact = await GetPartnerContactAsync();
            if (contact == null) return View("NotAPartner");

            var account = await _context.Set<SupplierCurrentAccount>()
                .Include(a => a.Transactions)
                    .ThenInclude(t => t.Project)
                .FirstOrDefaultAsync(a => a.PhonebookContactId == contact.Id);

            if(account == null)
            {
                // Hesabı henüz oluşturulmamış (İşlem yapılmamış tedarikçi)
                ViewBag.NoAccount = true;
                return View();
            }

            account.Transactions = account.Transactions.OrderByDescending(t => t.TransactionDate).ToList();
            ViewBag.Contact = contact;

            return View(account);
        }

        public async Task<IActionResult> Contracts()
        {
            var contact = await GetPartnerContactAsync();
            if (contact == null) return View("NotAPartner");

            var contracts = await _context.Set<SubcontractorContract>()
                .Include(c => c.Project)
                .Include(c => c.Hakedisler)
                .Where(c => c.SubcontractorId == contact.Id)
                .ToListAsync();

            ViewBag.Contact = contact;
            return View(contracts);
        }
    }
}
'''

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
