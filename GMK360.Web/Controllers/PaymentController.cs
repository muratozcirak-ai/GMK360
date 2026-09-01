using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GMK360.Web.Services.Integration;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IyzicoPaymentService _paymentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public PaymentController(IyzicoPaymentService paymentService, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _paymentService = paymentService;
            _userManager = userManager;
            _db = db;
        }

        // GET: /Payment/Checkout
        public async Task<IActionResult> Checkout(decimal amount, string subMerchantKey, string description)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null) return Unauthorized();

            ViewBag.WalletBalance = user.RealMoneyBalance;
            ViewBag.Amount = amount;
            ViewBag.SubMerchantKey = subMerchantKey;
            ViewBag.Description = description;
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(decimal amount, string subMerchantKey, string cardHolderName, string cardNumber, string expireMonth, string expireYear, string cvc, bool useWallet = false)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null) return Unauthorized();

            decimal amountToPayWithCard = amount;
            decimal amountPaidFromWallet = 0;
            bool isFullWalletPayment = false;

            if (useWallet && user.RealMoneyBalance > 0)
            {
                if (user.RealMoneyBalance >= amount)
                {
                    // Tamamen cüzdandan ödeme
                    amountPaidFromWallet = amount;
                    user.RealMoneyBalance -= amount;
                    isFullWalletPayment = true;
                    amountToPayWithCard = 0;
                }
                else
                {
                    // Kısmi ödeme
                    amountPaidFromWallet = user.RealMoneyBalance;
                    amountToPayWithCard = amount - user.RealMoneyBalance;
                    user.RealMoneyBalance = 0;
                }

                // Cüzdan hareketini kaydet
                _db.UserWalletTransactions.Add(new UserWalletTransaction
                {
                    ApplicationUserId = user.Id,
                    Amount = -amountPaidFromWallet, // Negatif değer (Harcama)
                    TransactionType = "Payment",
                    Description = $"Cüzdan Bakiyesi ile İç Ödeme: {ViewBag.Description ?? "Hizmet/Aidat Ödemesi"} ({amountPaidFromWallet:C2})"
                });
                
                await _userManager.UpdateAsync(user);
                await _db.SaveChangesAsync();
            }

            // Kredi kartı ile çekilecek tutar varsa Iyzico'ya git
            if (!isFullWalletPayment && amountToPayWithCard > 0)
            {
                var success = await _paymentService.ProcessSplitPaymentAsync(
                    amountToPayWithCard,
                    subMerchantKey ?? "T12345", // Mock SubMerchant
                    cardHolderName,
                    cardNumber?.Replace(" ", "") ?? "",
                    expireMonth ?? "",
                    expireYear ?? "",
                    cvc ?? ""
                );

                if (!success)
                {
                    TempData["ErrorMessage"] = "Ödeme işlemi sırasında bir hata oluştu. Lütfen bilgilerinizi kontrol edin.";
                    return RedirectToAction("Checkout", new { amount = amount, subMerchantKey = subMerchantKey, description = "Tekrar Deneme" });
                }
            }

            TempData["SuccessMessage"] = "Ödemeniz başarıyla alındı ve uzmana iletildi!";
            return RedirectToAction("Success");
        }
        
        public IActionResult Success()
        {
            return View();
        }
    }
}
