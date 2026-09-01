namespace GMK360.Core.Entities
{
    public class MaterialPriceInquiry : BaseEntity
    {
        public string TradesmanUserId { get; set; } // Fiyatı soran usta
        public Identity.ApplicationUser TradesmanUser { get; set; }

        public string SupplierUserId { get; set; } // Fiyatı sorulan esnaf
        public Identity.ApplicationUser SupplierUser { get; set; }

        public string MaterialDescription { get; set; } // Örn: 10 kutu beyaz tavan boyası
        
        public decimal? QuotedPrice { get; set; } // Esnafın verdiği fiyat (doldurana kadar null)
        
        public bool IsResolved { get; set; } = false; // Süreç tamamlandı mı?
    }
}
