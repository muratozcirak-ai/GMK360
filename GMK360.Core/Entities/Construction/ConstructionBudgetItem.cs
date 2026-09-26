using System;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.B2b;

namespace GMK360.Core.Entities.Construction
{
    public enum BudgetItemSourceType
    {
        Manual = 1,
        Warehouse = 2,
        DXF = 3
    }

    public enum BudgetQuoteStatus
    {
        WaitingForPrice = 1, // Sadece miktar var, fiyat 0 (Aşama 1)
        EstimatedOrQuoted = 2, // Teklif alındı veya müteahhit tecrübesine göre planlandı (Aşama 2)
        ActualInvoiced = 3 // Faturası kesildi, gerçekleşti (Aşama 3)
    }

    public enum BudgetPhaseCategory
    {
        ResmiIslemlerVeKurulum = 1, // Resmi İşlemler, Proje ve Şantiye Kurulumu
        YikimHafriyatZemin = 2, // Yıkım, Hafriyat ve Zemin İşleri
        KabaInsaat = 3, // Kaba İnşaat (Ana Taşıyıcılar)
        ElektrikZayifAkim = 4, // Elektrik ve Zayıf Akım Tesisatı
        MekanikSihhiTesisat = 5, // Mekanik ve Sıhhi Tesisat
        DisCepheYalitimCati = 6, // Dış Cephe, Yalıtım ve Çatı
        InceIscilikKaplama = 7, // İnce İşçilik, Kaplama ve Montaj
        CevreDuzenlemeTeslim = 8 // Çevre Düzenleme ve Teslim
    }

    public class ConstructionBudgetItem : BaseEntity
    {
        public int ConstructionProjectId { get; set; }
        public ConstructionProject ConstructionProject { get; set; }

        public BudgetPhaseCategory PhaseCategory { get; set; }
        
        public string ItemName { get; set; } // Örn: C30 Beton, Mikser Yolu Dökümü
        public string? Description { get; set; } // Detay/Notlar
        
        public decimal Quantity { get; set; } = 1;
        public string Unit { get; set; } = "Adet"; // m2, m3, Ton, Adet vs.

        public BudgetItemSourceType SourceType { get; set; } = BudgetItemSourceType.Manual;
        public BudgetQuoteStatus QuoteStatus { get; set; } = BudgetQuoteStatus.WaitingForPrice;

        // BÜTÇE RAKAMLARI
        public decimal PlannedUnitPrice { get; set; } = 0; // Planlanan/Tahmini Birim Fiyat
        [NotMapped]
        public decimal PlannedTotalCost => Quantity * PlannedUnitPrice; // Planlanan Toplam
        
        public decimal ActualTotalCost { get; set; } = 0; // Gerçekleşen Toplam Harcama (Fatura Tutarı)
        
        // ÖNGÖRÜLMEYEN GİDER (Mikser Yolu örneği gibi planda olmayan sonradan çıkan giderler)
        public bool IsUnplannedExtra { get; set; } = false;

        // DİJİTAL İKİZ VE DEMİRBAŞ BAĞLANTISI (Faz 3 için altyapı)
        public int? AssetDetailsId { get; set; } 

        // TEDARİKÇİ / TAŞERON BAĞLANTISI
        public int? SupplierId { get; set; }
        public B2bCompany Supplier { get; set; }
    }
}
