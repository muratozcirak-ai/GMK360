using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    public class Complex : BaseEntity
    {
        public string Name { get; set; }
        
        public bool IsSite { get; set; } = true; // Gerçek bir site mi yoksa bağımsız bina mı?
        public string BuildingNumber { get; set; } // Dış Kapı No (Site değilse doldurulur)

        public int CityId { get; set; }
        public City City { get; set; }

        public int DistrictId { get; set; }
        public District District { get; set; }

        public int NeighborhoodId { get; set; }
        public Neighborhood Neighborhood { get; set; }

        public int? StreetId { get; set; }
        public Street Street { get; set; }
        
        // Harita Koordinatları (Kalıcı Demirbaş)
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        // Tesisin Sabit Özellikleri
        public int? TotalUnits { get; set; }
        public int? TotalFloors { get; set; } // Binadaki Toplam Kat Sayısı (Tekil binalar için)
        public bool HasBlocks { get; set; } // Bloklu bir yapı mı?
        public bool HasSecurity { get; set; }
        public bool HasPool { get; set; }
        
        // Sistemin güvenliği için Admin onayı
        public bool IsApproved { get; set; } = false; 

        // Navigation Property
        public ICollection<Property> Properties { get; set; }
        public ICollection<ComplexFeature> Features { get; set; }
        public ICollection<ComplexBlock> Blocks { get; set; }
    }
}
