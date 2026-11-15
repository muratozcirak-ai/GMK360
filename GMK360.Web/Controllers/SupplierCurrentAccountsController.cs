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
    public class SupplierCurrentAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SupplierCurrentAccountsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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

        // GET: /SupplierCurrentAccounts/Index
        public async Task<IActionResult> Index()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var accounts = await _context.SupplierCurrentAccounts
                .Include(a => a.PhonebookContact)
                .Where(a => a.AgencyId == agencyId.Value)
                .OrderBy(a => a.PhonebookContact.Name)
                .ToListAsync();

            return View(accounts);
        }

        // GET: /SupplierCurrentAccounts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var account = await _context.SupplierCurrentAccounts
                .Include(a => a.PhonebookContact)
                .Include(a => a.Transactions)
                .Include(a => a.Payments)
                .FirstOrDefaultAsync(a => a.Id == id && a.AgencyId == agencyId.Value);

            if (account == null) return NotFound();

            // Sadece bu view'da kolay gösterim için transaction ve ödemeleri birleştirebiliriz
            // ya da view içinde ayrı ayrı sekmelerde veya birleşik tabloda gösterebiliriz.

            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(int accountId, decimal amount, PaymentMethod method, DateTime paymentDate, string checkNumber, DateTime? dueDate, string notes)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var account = await _context.SupplierCurrentAccounts
                .FirstOrDefaultAsync(a => a.Id == accountId && a.AgencyId == agencyId.Value);

            if (account == null) return NotFound();

            var userId = _userManager.GetUserId(User) ?? "";

            // 1. Ödeme Kaydını Oluştur
            var payment = new SupplierPayment
            {
                SupplierCurrentAccountId = account.Id,
                Amount = amount,
                Method = method,
                PaymentDate = paymentDate,
                CheckNumber = checkNumber,
                DueDate = dueDate,
                Notes = notes ?? "",
                Status = PaymentStatus.Completed,
                HandledByUserId = userId
            };

            // Eğer çek ise, status pending olabilir ama basitlik adına şimdilik completed/pending ayrımını UI'a bırakalım
            if (method == PaymentMethod.Check && dueDate.HasValue && dueDate.Value > DateTime.UtcNow)
            {
                payment.Status = PaymentStatus.Pending;
            }

            _context.SupplierPayments.Add(payment);

            // 2. Bakiyeyi Düş (Ödeme yaptık, borcumuz azaldı)
            account.CurrentBalance -= amount;

            // 3. Transaction Tablosuna Yaz (Ekstre için)
            var transaction = new SupplierAccountTransaction
            {
                SupplierCurrentAccountId = account.Id,
                Amount = amount,
                Type = SupplierTransactionType.PaymentMade,
                TransactionDate = paymentDate,
                Description = $"{method} ile ödeme yapıldı. {(string.IsNullOrEmpty(checkNumber) ? "" : "Çek No: " + checkNumber)}",
                BalanceAfterTransaction = account.CurrentBalance,
                CreatedByUserId = userId
            };

            _context.SupplierAccountTransactions.Add(transaction);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ödeme başarıyla kaydedildi ve bakiye güncellendi.";
            return RedirectToAction(nameof(Details), new { id = accountId });
        }
    }
}



