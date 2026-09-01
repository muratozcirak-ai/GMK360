using System;

namespace GMK360.Core.DTOs
{
    public class MarketDataIncomingDto
    {
        public string City { get; set; }
        public string District { get; set; }
        public string Neighborhood { get; set; }
        public string PropertyType { get; set; } // Daire, Arsa, Dükkan vb.
        public decimal Price { get; set; }
        public double SquareMeters { get; set; }
        public string OriginalSourceId { get; set; } // Karşı sitedeki ilan no (Mükerrer kaydı önlemek için)
        
        // YENİ EKLENENLER: Kaynak takibi için
        public string SourcePlatform { get; set; } // Örn: "Sahibinden", "Emlakjet"
        public string SourceUrl { get; set; }      // Tıklayıp manuel kontrol edeceğimiz ilan linki
    }
}
