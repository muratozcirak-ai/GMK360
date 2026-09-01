using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FinancialReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FinancialReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Sisteme yüklenen toplam fon (Tüm kullanıcı cüzdanlarındaki bakiyeler)
            var totalUserBalances = await _context.Users.SumAsync(u => u.WalletBalance);

            // Örnek bir fon dağılım raporu için model oluşturacağız.
            // İleride bu veriler Ledger (Defter) tablolarından çekilebilir.
            
            // Vergi kuralları aktif oranları (örneğin stopaj %15 vb.)
            var activeRules = await _context.GlobalObligationRules
                .Where(r => r.IsActive)
                .ToListAsync();

            ViewBag.TotalUserBalances = totalUserBalances;
            ViewBag.ActiveRules = activeRules;
            
            // Şimdilik mock/summary bir veri gönderiyoruz. Asıl tablo yapısı tam oturunca burası güncellenecek.
            return View();
        }
    }
}
