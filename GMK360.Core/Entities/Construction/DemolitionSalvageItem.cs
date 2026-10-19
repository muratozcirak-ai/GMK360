using System;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction
{
    // Eski Bina Söküm ve Değerli Hurda Envanteri (Kentsel Dönüşüm / Yıkım Öncesi)
    public class DemolitionSalvageItem : BaseEntity
    {
        public int ConstructionProjectId { get; set; }
        public ConstructionProject ConstructionProject { get; set; }

        public string ItemCategory { get; set; } // "Kapı & Pencere", "Isıtma & Tesisat", "Mutfak & Banyo", "Yapısal Hurda", "Diğer"
        public string ItemName { get; set; }     // Örn: "Panel Radyatör Petek", "Çelik Kapı", "Kombi", "PVC Doğrama", "Hurda Demir"
        
        public decimal EstimatedQuantity { get; set; } // Tahmini Miktar (örn: 45)
        public decimal? ActualQuantity { get; set; }    // Gerçek Sökülen Miktar (örn: 48)
        public string Unit { get; set; } = "Adet";     // "Adet", "Kg", "Ton", "m²", "Takım"
        
        public decimal? EstimatedUnitPrice { get; set; } // Tahmini Birim Fiyat (TL)
        public decimal? EstimatedTotalPrice => EstimatedQuantity * (EstimatedUnitPrice ?? 0);
        
        public string Status { get; set; } = "Binada Takılı"; // "Binada Takılı", "Söküldü / Depoda", "Satıldı", "Yıkımcıya Devredildi"
        
        public decimal? SoldTotalPrice { get; set; } // Satıştan Elde Edilen Net Tutar (TL)
        public string? BuyerName { get; set; }       // Alan Hurdacı / Firma
        public DateTime? SoldDate { get; set; }
        
        public string? Notes { get; set; }
    }
}
