using System;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction
{
    public enum ConstructionExpenseType
    {
        // Ön Hazırlık & Resmi Harçlar
        BelediyeHarci = 10,         // Belediye Harçları & İmar Çapı
        MuhendislikEtud = 11,       // Mühendislik / Zemin Sondajı & Jeoteknik
        MimariTasarim = 12,         // Mimari & Statik Tasarım / Avan Proje
        YapiDenetim = 13,           // Yapı Denetim & Ruhsat Harçları
        
        // Şantiye & İmalat
        SantiyeKurulum = 14,        // Şantiye Kurulum & Mobilizasyon
        MalzemeHakedis = 15,        // Malzeme Alımı & Taşeron Hakedişi
        
        // Genel & Şantiye İçi
        SantiyeIasesi = 1,          // Şantiye Yemek/Çay/Su
        TemsilAgirlama = 2,         // Müşteri Yemek, Lansman vs
        DigerGenelGider = 3,        // Ofis, Kırtasiye, Ulaşım
        SirketIciYemek = 4,         // Şirket İçi Yemek
        Iletisim = 5,               // İletişim (Telefon / İnternet)
        Demirbas = 6,               // Demirbaş
        SarfMalzeme = 7,            // Sarf Malzeme
        Temizlik = 8,               // Temizlik
        Diger = 9                   // Diğer
    }

    public class ConstructionProjectExpense : BaseEntity
    {
        public int AgencyId { get; set; }
        public int? ProjectId { get; set; }
        public string? PhotoPath { get; set; }
        public virtual ConstructionProject? Project { get; set; }
        
        public ConstructionExpenseType ExpenseType { get; set; }
        public string Title { get; set; } // Örn: Zemin Sondajı ve Raporu
        public string? Description { get; set; }
        
        // Altın Kural: KDV'siz Net Tutar Esas Alınır
        public decimal? AmountExcludingVat { get; set; } // KDV Hariç Net Tutar (Esas Maliyet)
        public int VatRate { get; set; } = 0;           // KDV Oranı (%0, %1, %10, %20)
        public decimal? VatAmount { get; set; }          // Ayrılan KDV Tutarı
        public decimal Amount { get; set; }             // KDV Dahil Toplam Belge Tutarı (Fiili Çıkış)
        
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        public bool IsPaid { get; set; } = true;
        public string? DocumentNo { get; set; }         // Fatura/Fiş/Makbuz No
        public string? PaymentSource { get; set; }      // Proje Kasası, Şirket Kasası, Banka
        public string? PayeeOrSupplier { get; set; }    // Kime Ödendi / Tedarikçi / Kurum

        // Ortaklık / Joint Venture Dağılımı
        public bool IsJointVenture { get; set; } = false;
        public string? PartnerCompanyName { get; set; }
        public decimal OurSharePercentage { get; set; } = 100m;
        public decimal PartnerSharePercentage { get; set; } = 0m;
        public decimal? OurShareAmount { get; set; }
        public decimal? PartnerShareAmount { get; set; }

        public string? Currency { get; set; } = "TRY";
        public decimal? ExchangeRate { get; set; } = 1m;
        public decimal? CurrencyAmount { get; set; }

        public string? SiteDiaryNote { get; set; }
        public DateTime? SiteDiaryDate { get; set; }
        public string ApprovalStatus { get; set; } = "Approved";
        public string? ApprovalType { get; set; }
        public string? ApprovalPin { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
    }
}
