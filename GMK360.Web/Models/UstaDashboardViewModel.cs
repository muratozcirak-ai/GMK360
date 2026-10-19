using System;
using System.Collections.Generic;
using GMK360.Core.Entities;

namespace GMK360.Web.Models
{
    public enum TaxShieldOption
    {
        NetAgreed_TaxOnClient,  // Tavsiye Edilen: Net Ele Geçecek (Stopaj & Gider Pusulası Yönetime/Müşteriye Ait)
        WithholdingByClient,   // Stopaj Müşteriden (Müşteri Gider Pusulası Keserse Vergiyi Kendi Karşılar)
        InvoiceWithVat          // Faturalı (+ %20 KDV Eklenir)
    }

    public class UstaDashboardViewModel
    {
        public int UstaId { get; set; }
        public string FullName { get; set; } = "Ahmet Usta";
        public string TradeTitle { get; set; } = "Elektrik & Sıhhi Tesisat Uzmanı";
        public string PhoneNumber { get; set; } = "0532 999 88 77";
        public string CityName { get; set; } = "İstanbul";
        public string DistrictName { get; set; } = "Kadıköy";
        public string NeighborhoodName { get; set; } = "Fenerbahçe Mahallesi";
        public string Bio { get; set; } = "20 yılı aşkın tecrübe ile Kadıköy ve çevresinde konut, iş yeri ve apartman elektrik panosu, aydınlatma ve tesisat revizyonları.";
        
        public bool IsVerified { get; set; } = true;
        public int ReliabilityScore { get; set; } = 98; // %98
        public double AverageRating { get; set; } = 4.9;
        public int TotalReviewsCount { get; set; } = 48;
        public bool IsSampleData { get; set; } = true;

        // Freemium & Aidat Rozetleri
        public bool IsMahalleFreemiumActive { get; set; } = true; // Kendi mahallesinde %100 ÜCRETSİZ
        public bool IsCityWideSubscriptionActive { get; set; } = false; // Tüm şehir & yan ilçeler
        public decimal CityWideMonthlyFee { get; set; } = 250m; // Tek düz sabit aidat (250 TL/ay)

        // Belgeler & Sertifikalar
        public List<UstaDocumentViewModel> Documents { get; set; } = new List<UstaDocumentViewModel>();

        // Referans İşler
        public List<UstaPortfolioViewModel> Portfolios { get; set; } = new List<UstaPortfolioViewModel>();

        // Ustanın Net Kâr Pusulası (Cebime Kalan Reel Para)
        public int TotalJobsCount => Jobs.Count;
        public int ActiveJobsCount { get; set; }
        public int CompletedJobsCount { get; set; }
        public decimal TotalAgreedRevenue { get; set; }
        public decimal TotalMaterialExpense { get; set; }
        public decimal TotalAssistantExpense { get; set; }
        public decimal TotalRoadFoodExpense { get; set; }
        public decimal TotalNetProfit { get; set; }
        public double NetProfitMarginPercent => TotalAgreedRevenue > 0 ? (double)(TotalNetProfit / TotalAgreedRevenue * 100) : 0;

        // İş Masası (Fatura & Stopaj Kalkanı)
        public List<UstaJobCardViewModel> Jobs { get; set; } = new List<UstaJobCardViewModel>();

        // Nalbur & Tedarikçi Açık Hesap (Cari Defteri)
        public UstaNalburAccountViewModel NalburAccount { get; set; } = new UstaNalburAccountViewModel();
        public List<UstaNalburTransactionViewModel> NalburTransactions { get; set; } = new List<UstaNalburTransactionViewModel>();

        // Tedarikçi Malzeme Fiyat Araştırma Kataloğu
        public List<UstaMaterialPriceItemViewModel> MaterialCatalog { get; set; } = new List<UstaMaterialPriceItemViewModel>();

        // Çırak / Kalfa Ekibi (Organik Ağın Büyümesi)
        public List<UstaAssistantViewModel> Assistants { get; set; } = new List<UstaAssistantViewModel>();
    }

    public class UstaDocumentViewModel
    {
        public string Title { get; set; }
        public string Issuer { get; set; } // Örn: Mesleki Yeterlilik Kurumu (MYK), MEB
        public string DocumentNumber { get; set; }
        public string Year { get; set; }
        public bool IsApproved { get; set; } = true;
    }

    public class UstaPortfolioViewModel
    {
        public string Title { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string BeforeImageUrl { get; set; }
        public string AfterImageUrl { get; set; }
        public string ClientReference { get; set; }
        public bool IsVerifiedReference { get; set; } = true;
    }

    public class UstaJobCardViewModel
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;
        public string LocationAddress { get; set; } = null!;
        public decimal AgreedAmount { get; set; }

        public TaxShieldOption TaxShield { get; set; } = TaxShieldOption.NetAgreed_TaxOnClient;
        public string TaxShieldBadgeText { get; set; } = "🛡️ NET ANLAŞMA (Stopaj & Vergiler Yönetime Aittir)";
        public string TaxShieldDescription { get; set; } = "Bu tutar ustanın cebine geçecek net tutardır. Yönetimin keseceği gider pusulası stopajı hakedişten düşülemez.";

        public bool IsMaterialIncluded { get; set; } = false; // False: Sadece İşçilik / Malzeme Hariç
        public string PaymentMethod { get; set; } = "İş Bitimi Peşin / Havale";
        public string Status { get; set; } = "Devam Ediyor"; // Teklif Verildi, Devam Ediyor, Tamamlandı
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Harcamalar & Kâr
        public decimal MaterialCost { get; set; } // Nalburdan çekilen
        public decimal AssistantCost { get; set; } // Çırak yevmiyesi
        public decimal RoadFoodCost { get; set; } // Yol & Yemek masrafı
        public decimal NetProfit => AgreedAmount - (MaterialCost + AssistantCost + RoadFoodCost);
        public double NetProfitMargin => AgreedAmount > 0 ? (double)(NetProfit / AgreedAmount * 100) : 0;

        public string? Notes { get; set; }

        // WhatsApp Mutabakat Metni & URL
        public string WhatsAppMessage { get; set; } = "";
        public string WhatsAppUrl { get; set; } = "";
    }

    public class UstaNalburAccountViewModel
    {
        public int Id { get; set; }
        public string NalburName { get; set; } = "Özkan Yapı & Elektrik Nalburiye";
        public string ContactPerson { get; set; } = "Mustafa Özkan";
        public string PhoneNumber { get; set; } = "0533 111 22 33";
        public string District { get; set; } = "Kadıköy / Fenerbahçe";
        public decimal CurrentDebtBalance { get; set; } = 4800m; // Nalbura Olan Açık Hesap Borcu
        public string Status { get; set; } = "Açık Hesap (Veresiye) Aktif";
    }

    public class UstaNalburTransactionViewModel
    {
        public int Id { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string TransactionType { get; set; } = "Malzeme Çekildi (+Borç)"; // Malzeme Çekildi (+Borç) / Ödeme Yapıldı (-Borç)
        public decimal Amount { get; set; }
        public string Description { get; set; } = "";
        public string? RelatedJobTitle { get; set; }
        public int? RelatedJobId { get; set; }
        public decimal BalanceAfter { get; set; }
    }

    public class UstaAssistantViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "Ali Çetin";
        public string PhoneNumber { get; set; } = "0544 333 44 55";
        public string Role { get; set; } = "Elektrik Kalfası";
        public decimal DailyWage { get; set; } = 1200m;
        public int DaysWorkedTotal { get; set; } = 2;
        public decimal TotalWagesPaid => DaysWorkedTotal * DailyWage;
        public bool IsRegisteredFutureUsta { get; set; } = true; // GMK360 Aday Usta Havuzunda
        public DateTime RegisteredDate { get; set; } = DateTime.UtcNow;
    }

    public class UstaMaterialPriceItemViewModel
    {
        public string MaterialName { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public decimal AveragePrice { get; set; }
        public string SupplierName { get; set; }
        public string Location { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
