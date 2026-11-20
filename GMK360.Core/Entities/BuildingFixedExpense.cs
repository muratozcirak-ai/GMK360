using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    /// <summary>
    /// Aylık sabit bina giderleri (Asansör Bakım, Ortak Elektrik, Su, Doğalgaz, Muhasebe/Mali Müşavir vb.).
    /// "Isletme" (Oturan öder) veya "Demirbas" (Malik öder) kapsamı ile ödeme günü ajandası içerir.
    /// </summary>
    public class BuildingFixedExpense : BaseEntity
    {
        public int BuildingId { get; set; }
        [ForeignKey("BuildingId")]
        public virtual Building Building { get; set; } = null!;

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string ExpenseScope { get; set; } = "Isletme"; // "Isletme" | "Demirbas"

        [Required, MaxLength(30)]
        public string DistributionRule { get; set; } = "Esit"; // "Esit" | "ArsaPayi" | "Metrekare"

        [Range(1, 31)]
        public int DueDayOfMonth { get; set; } = 15;

        [MaxLength(150)]
        public string? ProviderName { get; set; } // Örn: Enerjisa, İSKİ, Kone, Mali Müşavir

        [MaxLength(150)]
        public string? SubscriberNo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyAmount { get; set; }

        public bool IsAutoRecurring { get; set; } = true;
        public bool IsActive { get; set; } = true;
    }
}
