using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public enum DeclarationStatus
    {
        Draft = 1,
        SubmittedToTaxOffice = 2
    }

    public enum DeductionMethod
    {
        LumpSum = 1,      // Götürü Gider (%15)
        RealExpense = 2   // Gerçek Gider (Faturalı)
    }

    public class IncomeTaxDeclaration : BaseEntity
    {
        public string OwnerUserId { get; set; } // Ev sahibi

        public int TaxYear { get; set; } // Örn: 2026

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalRentalIncome { get; set; } // Tüm evlerden gelen kira

        [Column(TypeName = "decimal(18,2)")]
        public decimal LegalExemptionAmount { get; set; } // Devletin muafiyet rakamı

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalWithholdingTaxPaidByTenants { get; set; } // Dükkan stopajı (mahsup)

        [Column(TypeName = "decimal(18,2)")]
        public decimal CalculatedTaxBase { get; set; } // Vergiye tabi matrah

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedTaxAmount { get; set; } // Dilimlere göre hesaplanan vergi

        public DeductionMethod DeductionMethod { get; set; } = DeductionMethod.LumpSum;

        public DeclarationStatus DeclarationStatus { get; set; } = DeclarationStatus.Draft;

        public string? DocumentUrl { get; set; }
    }
}
