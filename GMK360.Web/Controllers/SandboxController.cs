using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GMK360.Core.Interfaces;

namespace GMK360.Web.Controllers
{
    public class SandboxController : Controller
    {
        private readonly ITradesmanService _tradesmanService;
        private readonly IPaymentGatewayService _paymentService;

        public SandboxController(ITradesmanService tradesmanService, IPaymentGatewayService paymentService)
        {
            _tradesmanService = tradesmanService;
            _paymentService = paymentService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TestAssignTradesman(int renovationId, string tradesmanId)
        {
            var result = await _tradesmanService.AssignJobToTradesmanAsync(renovationId, tradesmanId);
            TempData["Message"] = result ? "Usta başarıyla atandı!" : "Atama başarısız oldu.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> TestCompleteJobAndSurvey(int renovationId, int rating, string comments)
        {
            var result = await _tradesmanService.CompleteJobAndTriggerSurveyAsync(renovationId, rating, comments);
            TempData["Message"] = result ? "İş tamamlandı ve anket oluşturuldu!" : "İşlem başarısız.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> TestMarketplacePayment(string senderId, int receiverAccountId, decimal amount)
        {
            try
            {
                var transaction = await _paymentService.ProcessMarketplacePaymentAsync(
                    senderId, 
                    receiverAccountId, 
                    amount, 
                    Core.Entities.PaymentContextType.Maintenance, 
                    Guid.NewGuid());
                
                TempData["Message"] = $"Ödeme başarılı! İşlem ID: {transaction.TransactionId}, Net Tutar: {transaction.NetReceiverAmount:C2}";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Ödeme hatası: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
