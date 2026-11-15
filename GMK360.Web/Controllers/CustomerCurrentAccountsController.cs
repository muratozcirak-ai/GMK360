using GMK360.Core.Entities.Finance;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities.Identity;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class CustomerCurrentAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerCurrentAccountsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var consultant = await _context.AgencyConsultants
                    .FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive);
                if (consultant != null && consultant.AgencyId > 0)
                    return consultant.AgencyId;
            }
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) 
                return 1;
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var accounts = await _context.CustomerCurrentAccounts
                .Include(a => a.CustomerUser)
                .Where(a => a.AgencyId == agencyId.Value)
                .OrderByDescending(a => a.CurrentBalance)
                .ToListAsync();

            return View(accounts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var account = await _context.CustomerCurrentAccounts
                .Include(a => a.CustomerUser)
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.Id == id && a.AgencyId == agencyId.Value);

            if (account == null) return NotFound();

            account.Transactions = account.Transactions.OrderByDescending(t => t.TransactionDate).ToList();

            return View(account);
        }

        [HttpPost]
        public async Task<IActionResult> AddTransaction(int accountId, decimal amount, bool isDebtToUs, string description)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var account = await _context.CustomerCurrentAccounts
                .FirstOrDefaultAsync(a => a.Id == accountId && a.AgencyId == agencyId.Value);

            if (account == null) return NotFound();

            if (isDebtToUs)
                account.CurrentBalance += amount; // Müşteriye mal verdik/veresiye yazdık, borcu arttı
            else
                account.CurrentBalance -= amount; // Müşteri ödeme yaptı, borcu azaldı

            var transaction = new CustomerAccountTransaction
            {
                CustomerCurrentAccountId = accountId,
                Amount = amount,
                IsDebtToUs = isDebtToUs,
                Description = description,
                BalanceAfterTransaction = account.CurrentBalance,
                TransactionDate = DateTime.UtcNow,
                CreatedByUserId = _userManager.GetUserId(User) ?? ""
            };

            _context.CustomerAccountTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kayıt (Veresiye/Tahsilat) deftere başarıyla işlendi.";
            return RedirectToAction(nameof(Details), new { id = accountId });
        }
    }
}
