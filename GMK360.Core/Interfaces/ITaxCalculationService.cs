using System.Threading.Tasks;

namespace GMK360.Core.Interfaces
{
    public class TaxSuggestionResult
    {
        public decimal TotalIncome { get; set; }
        public decimal LegalExemption { get; set; }
        
        public decimal LumpSumDeductionAmount { get; set; } // %15 Götürü tutarı
        public decimal RealExpenseDeductionAmount { get; set; } // Gerçek gider faturaları toplamı
        
        public decimal LumpSumEstimatedTax { get; set; }
        public decimal RealExpenseEstimatedTax { get; set; }
        
        public bool IsRealExpenseMoreProfitable { get; set; }
        public decimal SavingsAmount { get; set; }
    }

    public interface ITaxCalculationService
    {
        /// <summary>
        /// Kullanıcının o yılki gelir ve giderlerine bakarak Götürü mü Gerçek Gider mi daha avantajlı (Smart Suggestion) hesaplar.
        /// </summary>
        Task<TaxSuggestionResult> CalculateBestDeductionMethodAsync(string userId, int taxYear);
    }
}
