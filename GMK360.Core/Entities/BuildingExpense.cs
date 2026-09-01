using System;

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

        // PayerType: Kim Ödemeli? "Tenant", "Owner", "All"
        // Yeni Mimaride BuildingExpenseType üzerinden yürüyecek ama geriye dönük uyum için tutulabilir
        public string? PayerType { get; set; } 

        public BuildingExpenseType ExpenseType { get; set; } = BuildingExpenseType.Operational;
    }
}
