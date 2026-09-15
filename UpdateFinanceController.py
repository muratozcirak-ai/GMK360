import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\FinanceController.cs'

content = '''using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Finance;
using GMK360.Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class FinanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FinanceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) 
                return 1;
                
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var consultant = await _context.AgencyConsultants
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive);
                
            return consultant?.AgencyId;
        }

        // Ortak Kasa ve Banka Akışı
        public async Task<IActionResult> Cashflow()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var cashTransactions = await _context.Set<AgencyCashTransaction>()
                .Include(c => c.SupplierCurrentAccount).ThenInclude(s => s.PhonebookContact)
                .Include(c => c.AgencyWorker)
                .Include(c => c.AgencyConsultant)
                .Where(c => c.AgencyId == agencyId.Value)
                .OrderByDescending(c => c.PaymentDate)
                .ToListAsync();

            // Hesaplamalar
            ViewBag.TotalCashOut = cashTransactions.Where(c => c.TransactionType == AgencyCashTransactionType.WorkerAdvance || 
                                                               c.TransactionType == AgencyCashTransactionType.ConsultantAdvance ||
                                                               c.TransactionType == AgencyCashTransactionType.SupplierPayment ||
                                                               c.TransactionType == AgencyCashTransactionType.GeneralExpense)
                                                   .Sum(c => c.Amount); // Varsayılan olarak hep çıkış kabul ediyoruz basitlik için, ama eğer gelir varsa çıkartılır.

            return View(cashTransactions);
        }

        [HttpPost]
        public async Task<IActionResult> AddExpense(decimal amount, string description, PaymentMethod method)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var expense = new AgencyCashTransaction
            {
                AgencyId = agencyId.Value,
                TransactionType = AgencyCashTransactionType.GeneralExpense,
                Amount = amount,
                Description = description,
                Method = method,
                PaymentDate = DateTime.UtcNow,
                Status = PaymentStatus.Completed,
                HandledByUserId = _userManager.GetUserId(User)
            };

            _context.Set<AgencyCashTransaction>().Add(expense);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Gider kasaya işlendi.";
            return RedirectToAction(nameof(Cashflow));
        }
    }
}
'''
with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
