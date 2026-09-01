using System;
using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities
{
    public enum TargetPropertyType
    {
        OnlyResidential = 1, // Konut (Daire)
        OnlyCommercial = 2,  // İşyeri (Dükkan)
        All = 3              // Tümü
    }

    public enum ResponsibleRole
    {
        Owner = 1,
        Tenant = 2
    }

    public enum PaymentFrequency
    {
        Monthly = 1,
        Yearly = 2,
        Biannual = 3 // Yılda iki taksit
    }

    public class FinancialObligationType : BaseEntity
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } // Örn: Emlak Vergisi, GMSİ, ÇTV, İlan Reklam Vergisi, Stopaj

        public TargetPropertyType TargetPropertyType { get; set; }

        public ResponsibleRole ResponsibleRole { get; set; }

        public PaymentFrequency PaymentFrequency { get; set; }

        public int? FirstInstallmentMonth { get; set; } // Örn. 3 (Mart)
        
        public int? SecondInstallmentMonth { get; set; } // Örn. 11 (Kasım)

        public bool IsActive { get; set; } = true;
    }
}
