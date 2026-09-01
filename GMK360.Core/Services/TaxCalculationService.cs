using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;

namespace GMK360.Core.Services
{
    public class TaxCalculationService : ITaxCalculationService
    {
        private readonly DbContext _context;

        public TaxCalculationService(DbContext context)
        {
            _context = context;
        }

        public async Task<TaxSuggestionResult> CalculateBestDeductionMethodAsync(string userId, int taxYear)
        {
            // 1. O yıla ait istisna tutarını al
            var taxParam = await _context.Set<TaxParameter>().FirstOrDefaultAsync(t => t.TaxYear == taxYear);
            decimal exemption = taxParam?.ResidentialExemptionAmount ?? 33000m; // 2024 varsayılan (Örnek)

            // 2. Kullanıcının Konut tipi mülklerinden elde ettiği toplam tahsilatı bul
            var userResidentialProperties = await _context.Set<Property>()
                .Where(p => p.OwnerUserId == userId && p.Type != null && p.Type.Name.Contains("Konut"))
                .Select(p => p.Id)
                .ToListAsync();

            decimal totalIncome = await _context.Set<PropertyFinancialSchedule>()
                .Where(s => userResidentialProperties.Contains(s.PropertyId) 
                         && s.DueDate.Year == taxYear 
                         && s.Status == ScheduleStatus.Paid) // Sadece ödenen/tahsil edilenler beyan edilir
                .SumAsync(s => s.Amount);

            // 3. Kullanıcının girdiği geçerli (IsTaxDeductible = true) "Gerçek Gider" faturalarını topla
            decimal totalRealExpenses = await _context.Set<PropertyExpense>()
                .Where(e => userResidentialProperties.Contains(e.PropertyId)
                         && e.TaxYear == taxYear
                         && e.IsTaxDeductible)
                .SumAsync(e => e.Amount);

            // Eğer gelir muafiyetin altındaysa vergi çıkmaz
            if (totalIncome <= exemption)
            {
                return new TaxSuggestionResult
                {
                    TotalIncome = totalIncome,
                    LegalExemption = exemption,
                    LumpSumEstimatedTax = 0,
                    RealExpenseEstimatedTax = 0,
                    IsRealExpenseMoreProfitable = false,
                    SavingsAmount = 0
                };
            }

            // 4. Götürü Gider Hesabı (Kalan tutarın %15'i gider sayılır)
            decimal taxableIncomeAfterExemption = totalIncome - exemption;
            decimal lumpSumDeduction = taxableIncomeAfterExemption * 0.15m;
            decimal lumpSumTaxBase = taxableIncomeAfterExemption - lumpSumDeduction;
            decimal lumpSumTax = CalculateTaxFromBrackets(lumpSumTaxBase, taxParam);

            // 5. Gerçek Gider Hesabı
            // Not: Gerçek gider yönteminde istisnaya isabet eden giderler düşülemez (Orantı hesabı yapılır)
            // Orantı = Kalan Gelir / Toplam Gelir
            decimal deductibleRatio = taxableIncomeAfterExemption / totalIncome;
            decimal allowableRealExpense = totalRealExpenses * deductibleRatio;
            
            decimal realExpenseTaxBase = taxableIncomeAfterExemption - allowableRealExpense;
            if (realExpenseTaxBase < 0) realExpenseTaxBase = 0;
            
            decimal realExpenseTax = CalculateTaxFromBrackets(realExpenseTaxBase, taxParam);

            // Sonuç oluştur
            var result = new TaxSuggestionResult
            {
                TotalIncome = totalIncome,
                LegalExemption = exemption,
                LumpSumDeductionAmount = lumpSumDeduction,
                RealExpenseDeductionAmount = allowableRealExpense,
                LumpSumEstimatedTax = lumpSumTax,
                RealExpenseEstimatedTax = realExpenseTax,
                IsRealExpenseMoreProfitable = realExpenseTax < lumpSumTax,
                SavingsAmount = Math.Abs(lumpSumTax - realExpenseTax)
            };

            return result;
        }

        private decimal CalculateTaxFromBrackets(decimal taxBase, TaxParameter param)
        {
            // Gerçek projede JSON tablosundan okunarak artan oranlı (Progressive) vergi hesaplanır.
            // Örnek basit mock hesaplama:
            if (taxBase <= 110000) return taxBase * 0.15m;
            if (taxBase <= 230000) return 16500 + (taxBase - 110000) * 0.20m;
            if (taxBase <= 870000) return 40500 + (taxBase - 230000) * 0.27m;
            
            return 213300 + (taxBase - 870000) * 0.35m; 
        }
    }
}
