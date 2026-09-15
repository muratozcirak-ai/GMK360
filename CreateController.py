import codecs
import os

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\SupplierCurrentAccountController.cs'

content = '''using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Finance;

namespace GMK360.Web.Controllers
{
    public class SupplierCurrentAccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupplierCurrentAccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SupplierCurrentAccount
        public async Task<IActionResult> Index()
        {
            var accounts = await _context.Set<SupplierCurrentAccount>()
                .Include(a => a.PhonebookContact)
                .ToListAsync();
            
            return View(accounts);
        }

        // GET: SupplierCurrentAccount/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var account = await _context.Set<SupplierCurrentAccount>()
                .Include(a => a.PhonebookContact)
                .Include(a => a.Transactions)
                    .ThenInclude(t => t.Project)
                .Include(a => a.Payments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (account == null) return NotFound();

            ViewBag.Projects = await _context.Set<GMK360.Core.Entities.Construction.ConstructionProject>().ToListAsync();
            return View(account);
        }

        // POST: Add Open Account Transaction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOpenAccount(int SupplierCurrentAccountId, decimal Amount, string Description, int? ProjectId)
        {
            var account = await _context.Set<SupplierCurrentAccount>().FindAsync(SupplierCurrentAccountId);
            if (account == null) return NotFound();

            // Borcu artır (Bizim tedarikçiye olan borcumuz)
            account.CurrentBalance += Amount;

            var transaction = new SupplierAccountTransaction
            {
                SupplierCurrentAccountId = SupplierCurrentAccountId,
                Amount = Amount,
                Type = SupplierTransactionType.OpenAccountPurchase,
                Description = Description,
                ProjectId = ProjectId,
                TransactionDate = DateTime.UtcNow,
                BalanceAfterTransaction = account.CurrentBalance
            };

            _context.Add(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = SupplierCurrentAccountId });
        }

        // POST: Add Payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(int SupplierCurrentAccountId, decimal Amount, PaymentMethod Method, DateTime? DueDate, string ReferenceNumber, string BankName, string Description)
        {
            var account = await _context.Set<SupplierCurrentAccount>().FindAsync(SupplierCurrentAccountId);
            if (account == null) return NotFound();

            // Borcu düşür
            account.CurrentBalance -= Amount;

            var payment = new SupplierPayment
            {
                SupplierCurrentAccountId = SupplierCurrentAccountId,
                Amount = Amount,
                Method = Method,
                DueDate = DueDate,
                CheckNumber = ReferenceNumber,
                BankName = BankName,
                Notes = Description,
                PaymentDate = DateTime.UtcNow,
                Status = DueDate.HasValue ? PaymentStatus.Pending : PaymentStatus.Completed
            };

            _context.Add(payment);

            var transaction = new SupplierAccountTransaction
            {
                SupplierCurrentAccountId = SupplierCurrentAccountId,
                Amount = Amount,
                Type = SupplierTransactionType.PaymentMade,
                Description = Description + (DueDate.HasValue ? $" (Vade: {DueDate.Value.ToShortDateString()})" : ""),
                TransactionDate = DateTime.UtcNow,
                BalanceAfterTransaction = account.CurrentBalance
            };
            
            _context.Add(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = SupplierCurrentAccountId });
        }
    }
}
'''
with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
