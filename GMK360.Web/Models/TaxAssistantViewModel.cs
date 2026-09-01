using System;
using System.Collections.Generic;

namespace GMK360.Web.Models
{
    public class TaxAssistantViewModel
    {
        public int TaxYear { get; set; }
        public decimal TotalRentalIncome { get; set; }
        public decimal LegalExemptionAmount { get; set; }
        public decimal TotalExpenses { get; set; } // Gerçek giderler
        
        public decimal TaxBaseWithLumpSum { get; set; } // Götürü gider ile matrah
        public decimal TaxBaseWithRealExpense { get; set; } // Gerçek gider ile matrah
        
        public decimal EstimatedTaxWithLumpSum { get; set; } 
        public decimal EstimatedTaxWithRealExpense { get; set; }
        
        public bool IsRealExpenseBetter { get; set; }
        public decimal AdvantageAmount { get; set; }
    }
}
