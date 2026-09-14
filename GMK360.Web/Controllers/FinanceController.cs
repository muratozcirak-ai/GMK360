using System;
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

        // GET: Finance
        public async Task<IActionResult> Index()
        {
            var accounts = await _context.SupplierCurrentAccounts
                .Include(a => a.PhonebookContact)
                .OrderByDescending(a => Math.Abs(a.CurrentBalance))
                .ToListAsync();

            return View(accounts);
        }

        // GET: Finance/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var account = await _context.SupplierCurrentAccounts
                .Include(a => a.PhonebookContact)
                .Include(a => a.Transactions).ThenInclude(t => t.FinanceCategory)
                .Include(a => a.Payments)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (account == null) return NotFound();

                        var consultant = await _context.Set<GMK360.Core.Entities.AgencyConsultant>().FirstOrDefaultAsync(a => a.UserId == user.Id);
            if(consultant != null) {
                ViewBag.Categories = await _context.FinanceCategories.Where(c => c.AgencyId == consultant.AgencyId).ToListAsync();
            }

            return View(account);
        }

        // POST: Finance/AddTransaction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTransaction(int accountId, SupplierTransactionType type, decimal amount, string description, string documentReference, int? financeCategoryId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var account = await _context.SupplierCurrentAccounts.FindAsync(accountId);
            if (account == null) return NotFound();

            var transaction = new SupplierAccountTransaction
            {
                SupplierCurrentAccountId = accountId,
                Type = type,
                FinanceCategoryId = financeCategoryId,
                Amount = amount,
                Description = description,
                DocumentReference = documentReference,
                TransactionDate = DateTime.UtcNow,
                CreatedByUserId = user.Id
            };

            // Bakiye güncelleme mantýðý
            // Pozitif bakiye = Biz borçluyuz. Negatif bakiye = Biz alacaklýyýz (Karþý taraf bize borçlu).
            if (type == SupplierTransactionType.PurchaseInvoice)
            {
                // Mal/Hizmet aldýk, borcumuz arttý
                account.CurrentBalance += amount;
            }
            else if (type == SupplierTransactionType.RefundReceived)
            {
                // Hurda sattýk veya iade aldýk (Bizim alacaðýmýz doðdu, borcumuz azaldý)
                account.CurrentBalance -= amount;
            }
            else if (type == SupplierTransactionType.Adjustment)
            {
                // Düzeltme (Formdan +/- girilebilir, ama standart olarak borç artýrýr diyelim, 
                // ya da amount'un iþaretine göre)
                account.CurrentBalance += amount;
            }
            // PaymentMade (Ödeme Yapýldý) burada deðil, AddPayment tarafýnda iþlenecek ama manuel girilirse:
            else if (type == SupplierTransactionType.PaymentMade)
            {
                account.CurrentBalance -= amount; // Borcumuz azaldý
            }

            transaction.BalanceAfterTransaction = account.CurrentBalance;
            
            _context.SupplierAccountTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ýþlem baþarýyla eklendi.";
            return RedirectToAction(nameof(Details), new { id = accountId });
        }

        // POST: Finance/AddPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(int accountId, PaymentMethod method, decimal amount, DateTime? dueDate, string checkNumber, string bankName, string notes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var account = await _context.SupplierCurrentAccounts.FindAsync(accountId);
            if (account == null) return NotFound();

            var payment = new SupplierPayment
            {
                SupplierCurrentAccountId = accountId,
                Method = method,
                Amount = amount,
                DueDate = dueDate,
                CheckNumber = checkNumber,
                BankName = bankName,
                Notes = notes,
                PaymentDate = DateTime.UtcNow,
                HandledByUserId = user.Id,
                // Çek ileri tarihliyse Pending, nakitse Completed varsayýyoruz
                Status = (method == PaymentMethod.Check || method == PaymentMethod.PromissoryNote || method == PaymentMethod.OpenAccount) ? PaymentStatus.Pending : PaymentStatus.Completed
            };

            _context.SupplierPayments.Add(payment);

            // Eðer ödeme tamamlanmýþsa (Nakit/Havale) hemen bakiyeden düþ.
            // Çek ise, tahsil edildiðinde düþmesi gerekebilir ama piyasada çek verildiðinde borçtan düþülür.
            // Basitlik adýna çek de verilse borcumuz kapandý sayalým (Risk takibi ayrý).
            if(method != PaymentMethod.OpenAccount)
            {
                account.CurrentBalance -= amount;
                
                // Cari hareketi de oluþtur
                var transaction = new SupplierAccountTransaction
                {
                    SupplierCurrentAccountId = accountId,
                    Type = SupplierTransactionType.PaymentMade,
                    Amount = amount,
                    Description = method.ToString() + " ile Ödeme: " + notes,
                    DocumentReference = checkNumber,
                    TransactionDate = DateTime.UtcNow,
                    CreatedByUserId = user.Id,
                    BalanceAfterTransaction = account.CurrentBalance
                };
                _context.SupplierAccountTransactions.Add(transaction);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ödeme/Tahsilat baþarýyla iþlendi.";
            return RedirectToAction(nameof(Details), new { id = accountId });
        }
    }
}



