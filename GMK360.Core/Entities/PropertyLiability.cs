using System;

namespace GMK360.Core.Entities
{
    public class PropertyLiability : BaseEntity
    {
        public int PropertyId { get; set; }
        public int? UtilityCompanyId { get; set; } // Fatura/Abonelik ise
        public int? LiabilityTypeId { get; set; } // Vergi veya aidat ise
        
        public string Title { get; set; } = null!; // Örn: "Doğalgaz Faturası", "Emlak Vergisi 1. Taksit"
        public string ReferenceNumber { get; set; } = null!; // Abone No, Tesisat No veya Sicil No
        
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
        public decimal? RecurringAmount { get; set; } // Aidat gibi sabit ödemeler için
        public int? DueDayOfMonth { get; set; } // Her ayın kaçında ödeniyor (Opsiyonel)

        public virtual Property Property { get; set; } = null!;
        public virtual UtilityCompany? UtilityCompany { get; set; }
        public virtual LiabilityType? LiabilityType { get; set; }
    }
}
