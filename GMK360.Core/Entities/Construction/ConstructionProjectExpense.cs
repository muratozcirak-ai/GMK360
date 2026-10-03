using System;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction
{
    public enum ConstructionExpenseType
    {
        SantiyeIasesi = 1,      // Şantiye Yemek/Çay/Su
        TemsilAgirlama = 2,     // Müşteri Yemek, Lansman, Kanepe vs
        DigerGenelGider = 3,    // Ofis, Kırtasiye, Ulaşım
        SirketIciYemek = 4,     // Şirket İçi Yemek
        Iletisim = 5,           // İletişim (Telefon / İnternet)
        Demirbas = 6,           // Demirbaş
        SarfMalzeme = 7,        // Sarf Malzeme
        Temizlik = 8            // Temizlik
    }

    public class ConstructionProjectExpense : BaseEntity
    {
        public int AgencyId { get; set; }
        public int? ProjectId { get; set; }
        public string? PhotoPath { get; set; }
        public ConstructionProject Project { get; set; }
        
        public ConstructionExpenseType ExpenseType { get; set; }
        public string Title { get; set; } // Örn: 15 Günlük Toplu Tabldot
        public string? Description { get; set; }
        
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        
        public bool IsPaid { get; set; } = false;
        public string? DocumentNo { get; set; } // Fatura/Fiş No
    }
}