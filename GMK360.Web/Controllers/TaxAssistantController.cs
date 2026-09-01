using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using GMK360.Web.Models;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class TaxAssistantController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaxAssistantController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Simulator(int year = 2026)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // 1. Kullanıcının Mülklerini Bul
            var properties = await _context.Properties
                .Where(p => p.UserId == user.Id)
                .Select(p => p.Id)
                .ToListAsync();

            if (!properties.Any())
            {
                // Mülkü yoksa uyar
                return RedirectToAction("Index", "DigitalHome");
            }

            // 2. O Yıla Ait Toplam Kira Geliri Hesaplama (Sözleşmeler üzerinden)
            // Kira bedeli * 12 şeklinde basitleştirilmiş bir hesaplama (veya sözleşme aktif mi diye kontrol edilir)
            var activeContracts = await _context.Set<TenancyContract>()
                .Where(c => properties.Contains(c.PropertyId) && c.IsActive)
                .ToListAsync();

            decimal totalRentalIncome = 0;
            foreach(var contract in activeContracts)
            {
                // İlgili yılda sözleşmenin kaç ay geçerli kaldığını varsayalım (şimdilik tüm yıl diyelim)
                totalRentalIncome += contract.MonthlyRentAmount * 12;
            }

            // 3. O Yıla Ait Gerçek Giderler
            var totalExpenses = await _context.Set<PropertyExpense>()
                .Where(e => properties.Contains(e.PropertyId) && e.TaxYear == year && e.IsTaxDeductible)
                .SumAsync(e => e.Amount);

            // 4. Devletin İstisna Tutarı (TaxParameters tablosundan)
            var taxParam = await _context.TaxParameters.FirstOrDefaultAsync(t => t.TaxYear == year);
            decimal exemptionAmount = taxParam != null ? taxParam.ResidentialExemptionAmount : 33000m; // Varsayılan 33.000 TL

            // 5. Vergi Hesaplama Mantığı
            // Eğer toplam kira, istisnadan düşükse beyannameye gerek yok
            decimal taxBaseBase = totalRentalIncome - exemptionAmount;
            if (taxBaseBase < 0) taxBaseBase = 0;

            // Yöntem 1: Götürü Gider (%15)
            decimal lumpSumExpense = taxBaseBase * 0.15m;
            decimal taxBaseWithLumpSum = taxBaseBase - lumpSumExpense;

            // Yöntem 2: Gerçek Gider (Veritabanındaki giderler)
            // Gerçek giderin istisnaya isabet eden kısmı düşülemez kuralı vardır ancak burada simülasyon yapıyoruz.
            // (Basitleştirilmiş GİB mantığı: İstisna sonrası matrahtan, giderin orantılı kısmı düşülür. Biz şimdilik direkt düşüyoruz)
            decimal taxBaseWithRealExpense = taxBaseBase - totalExpenses;
            if (taxBaseWithRealExpense < 0) taxBaseWithRealExpense = 0;

            var vm = new TaxAssistantViewModel
            {
                TaxYear = year,
                TotalRentalIncome = totalRentalIncome,
                LegalExemptionAmount = exemptionAmount,
                TotalExpenses = totalExpenses,
                TaxBaseWithLumpSum = taxBaseWithLumpSum,
                TaxBaseWithRealExpense = taxBaseWithRealExpense,
                EstimatedTaxWithLumpSum = CalculateTax(taxBaseWithLumpSum),
                EstimatedTaxWithRealExpense = CalculateTax(taxBaseWithRealExpense)
            };

            vm.IsRealExpenseBetter = vm.EstimatedTaxWithRealExpense < vm.EstimatedTaxWithLumpSum;
            vm.AdvantageAmount = vm.EstimatedTaxWithLumpSum - vm.EstimatedTaxWithRealExpense;

            return View(vm);
        }

        // Basit Vergi Dilimi Hesaplaması (2025/2026 Varsayılan Dilimler)
        private decimal CalculateTax(decimal taxBase)
        {
            if (taxBase <= 0) return 0;
            
            decimal tax = 0;
            
            if (taxBase <= 110000)
            {
                tax = taxBase * 0.15m;
            }
            else if (taxBase <= 230000)
            {
                tax = (110000 * 0.15m) + ((taxBase - 110000) * 0.20m);
            }
            else if (taxBase <= 870000)
            {
                tax = (110000 * 0.15m) + (120000 * 0.20m) + ((taxBase - 230000) * 0.27m);
            }
            else
            {
                // Daha üst dilimler... Şimdilik %35 sabit diyelim üstü için
                tax = (110000 * 0.15m) + (120000 * 0.20m) + (640000 * 0.27m) + ((taxBase - 870000) * 0.35m);
            }

            return tax;
        }
    }
}
