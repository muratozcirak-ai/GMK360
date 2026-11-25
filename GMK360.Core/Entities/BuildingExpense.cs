using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public enum ExpenseScopeType
    {
        HousingComplex = 1, // Site geneli (Örn: Havuz bakımı)
        Building = 2,       // Sadece bu bina/blok (Örn: Çatı yalıtımı, Asansör)
        Unit = 3            // Bireysel Daire içi (Örn: Daire içi boya)
    }

    public enum BuildingExpenseType
    {
        CapitalExpenditure = 1, // Demirbaş / Yatırım (Owner öder)
        Operational = 2         // Aidat / İşletme / Rutin (Tenant öder)
    }

    public class BuildingExpense : BaseEntity
    {
        
        public int? HousingComplexId { get; set; } // Site geneli ise
        public virtual HousingComplex HousingComplex { get; set; }

        public int? BuildingId { get; set; } // Bina geneli ise
        public virtual Building Building { get; set; }

        public ExpenseScopeType ScopeType { get; set; }

        public int? ExpenseCategoryId { get; set; }
        public virtual ExpenseCategory ExpenseCategory { get; set; }

        public int? InstitutionId { get; set; }
        public virtual Institution Institution { get; set; }

        public string Description { get; set; } // Örn: "Asansör Bakımı", "Ekim Ayı Yakıt Gideri"
        
        public decimal TotalAmount { get; set; } // Toplam Fatura Tutarı
        
        public DateTime Date { get; set; } = DateTime.Now;

        public string? InvoiceDocumentUrl { get; set; } // Orijinal ana faturanın sisteme yüklenmiş URL'si

        [MaxLength(250)]
        public string? PhysicalArchiveLocation { get; set; } // Islak imzalı asıl faturanın fiziksel klasör / raf yeri

        [MaxLength(40)]
        public string? LinkedSourceType { get; set; } // "Abonelik" | "ArizaIs" | "BakimDemirbas" | "Genel"

        public int? LinkedTaskTicketId { get; set; }
        public int? LinkedFixedExpenseId { get; set; }

        [MaxLength(250)]
        public string? LinkedSourceLabel { get; set; } // Örn: "Talep #12: Asansör Kapı Tamiri" veya "Enerjisa Elektrik"

        // PayerType: Kim Ödemeli? "Tenant", "Owner", "All"
        // Yeni Mimaride BuildingExpenseType üzerinden yürüyecek ama geriye dönük uyum için tutulabilir
        public string? PayerType { get; set; } 

        public BuildingExpenseType ExpenseType { get; set; } = BuildingExpenseType.Operational;

        [MaxLength(40)]
        public string ExpenseGroup { get; set; } = "FaturaAbonelik"; // "Personel" | "FaturaAbonelik" | "BakimOnarim" | "DemirbasYatirim" | "YonetimIdari"

        [MaxLength(40)]
        public string ExpenseItemKey { get; set; } = "Genel"; // "Genel" | "Personel" | "Asansor" | "Merdiven" | "CopTemizlik" | "Isinma" | "Demirbas"

        [MaxLength(30)]
        public string DistributionRule { get; set; } = "Esit"; // "Esit" | "ArsaPayi" | "Metrekare" | "Katsayi"

        [Column(TypeName = "decimal(6,2)")]
        public decimal ShopParticipationPercent { get; set; } = 100.00m;

        [Column(TypeName = "decimal(6,2)")]
        public decimal? OwnerSplitPercent { get; set; }

        // Değişmez Muhasebe Defteri: Fiziksel silme yasak, iptalde Ters Kayıt (Reversal Entry) açılır
        public bool IsArchived { get; set; } = false;
        public DateTime? ArchivedAt { get; set; }
        public string? ArchivedByUserId { get; set; }
        public int? ReversalOfExpenseId { get; set; }
    }
}
