using System;

namespace GMK360.Core.Entities
{
    public class MarketAnalytics
    {
        public int Id { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Neighborhood { get; set; }
        public string PropertyType { get; set; }
        public decimal Price { get; set; }
        public double SquareMeters { get; set; }
        public decimal PricePerSquareMeter { get; set; } // Sistem otomatik hesaplayacak
        public DateTime RecordDate { get; set; }
        public string OriginalSourceId { get; set; } 
        
        // YENİ EKLENENLER
        public string SourcePlatform { get; set; } 
        public string SourceUrl { get; set; }      
        
        public bool IsActive { get; set; } // Veriyi analize dahil edip etmeme durumu
    }
}
