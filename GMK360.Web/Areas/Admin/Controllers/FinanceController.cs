using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GMK360.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FinanceController : Controller
    {
        public IActionResult Index()
        {
            // İyzico entegrasyonu için finansal verilerin simülasyonu (Faz 3 Iyzico mock datası)
            var model = new FinanceDashboardViewModel
            {
                TotalRevenue = 150450.00m,
                PlatformCommission = 15045.00m,
                SubMerchantPayments = 135405.00m,
                PendingPayments = 3500.00m,
                RecentTransactions = new List<TransactionViewModel>
                {
                    new TransactionViewModel { Id = "TRX-001", Amount = 15000m, Commission = 1500m, SubMerchantAmount = 13500m, Status = "Başarılı", Date = "Bugün 14:30", SubMerchantName = "Ahmet Yılmaz (Danışman)" },
                    new TransactionViewModel { Id = "TRX-002", Amount = 8500m, Commission = 850m, SubMerchantAmount = 7650m, Status = "Başarılı", Date = "Dün 10:15", SubMerchantName = "Ayşe Kaya (Uzman)" },
                    new TransactionViewModel { Id = "TRX-003", Amount = 12000m, Commission = 1200m, SubMerchantAmount = 10800m, Status = "Beklemede", Date = "05 Ağu 2026", SubMerchantName = "Mehmet Demir (Danışman)" },
                    new TransactionViewModel { Id = "TRX-004", Amount = 35000m, Commission = 3500m, SubMerchantAmount = 31500m, Status = "Başarılı", Date = "03 Ağu 2026", SubMerchantName = "Emlak Ofisim A.Ş." }
                }
            };

            return View(model);
        }
    }

    public class FinanceDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal SubMerchantPayments { get; set; }
        public decimal PendingPayments { get; set; }
        public List<TransactionViewModel> RecentTransactions { get; set; }
    }

    public class TransactionViewModel
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public decimal Commission { get; set; }
        public decimal SubMerchantAmount { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }
        public string SubMerchantName { get; set; }
    }
}
