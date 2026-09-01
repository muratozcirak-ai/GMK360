using GMK360.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class ValuationController : Controller
    {
        private readonly IValuationService _valuationService;

        public ValuationController(IValuationService valuationService)
        {
            _valuationService = valuationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RequestValuation(string il, string ilce, string mahalle, string mulkTipi, string odaSayisi)
        {
            var userEmail = User.Identity.Name; 
            
            // Background task olarak tetiklemek icin Task.Run kullaniyoruz
            _ = Task.Run(() => _valuationService.RequestValuationAsync(il, ilce, mahalle, mulkTipi, odaSayisi, userEmail));

            TempData["SuccessMessage"] = "Talebiniz baþarýyla alýndý. Yapay zeka analiz raporunuz kýsa süre içinde e-posta adresinize gönderilecektir.";
            return RedirectToAction(nameof(Index));
        }
    }
}
