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

        [Required, MaxLength(40)]
        public string ExpenseGroup { get; set; } = "FaturaAbonelik"; // "Personel" | "FaturaAbonelik" | "BakimOnarim" | "DemirbasYatirim" | "YonetimIdari"

        [Required, MaxLength(40)]
        public string ExpenseItemKey { get; set; } = "Genel"; // "Genel" | "Personel" | "Asansor" | "Merdiven" | "CopTemizlik" | "Isinma" | "Demirbas"

        [Required, MaxLength(30)]
        public string DistributionRule { get; set; } = "Esit"; // "Esit" | "ArsaPayi" | "Metrekare" | "Katsayi"

        public ExpenseScopeType ScopeType { get; set; } = ExpenseScopeType.Building; // Building | HousingComplex

        [Column(TypeName = "decimal(6,2)")]
        public decimal ShopParticipationPercent { get; set; } = 100.00m; // Dükkan katılım % (0=Muaf, 50=İndirimli, 100=Normal, 150+=Artış)

        [Column(TypeName = "decimal(6,2)")]
        public decimal? OwnerSplitPercent { get; set; } // Null=standart, doluysa Malik payı % (100=Kiracı Muaf)

        [Range(1, 31)]
        public int DueDayOfMonth { get; set; } = 15;

        [MaxLength(150)]
        public string? ProviderName { get; set; } // Örn: Enerjisa, İSKİ, Kone, Mali Müşavir

        [MaxLength(150)]
        public string? SubscriberNo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyAmount { get; set; }

        public int BillingCycleMonths { get; set; } = 1; // 1: Aylik, 3: 3 Aylik, 6: 6 Aylik, 12: Yillik
        [Range(1, 12)]
        public int? BillingMonth { get; set; } // Yillik/donemsel odemede hangi ay (1-12)
        [MaxLength(500)]
        public string? ContractDocumentUrl { get; set; } // Sozlesme PDF veya taranmis foto
        [MaxLength(250)]
        public string? PhysicalArchiveLocation { get; set; } // Islak imzali fiziksel klasor/raf adresi
        public DateTime? ContractEndDate { get; set; }

        public bool IsAutoRecurring { get; set; } = true;
        public bool IsActive { get; set; } = true;
    }
}
