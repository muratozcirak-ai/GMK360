using GMK360.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities
{
    public class NeighborhoodPOI : BaseEntity
    {
        [Required]
        public string PoiName { get; set; } // Örn: Acıbadem Lisesi, Metro İstasyonu

        public POICategory PoiCategory { get; set; }

        public string? SubCategory { get; set; } // Opsiyonel detay: Örn "Devlet Hastanesi", "Otobüs Durağı"

        public double DistanceInMeters { get; set; } // Sokağa/Mahalleye olan kuş uçuşu mesafe
        
        public int? DurationInMinutes { get; set; } // Tahmini varış süresi (opsiyonel)

        public string? Latitude { get; set; }
        public string? Longitude { get; set; }

        public int NeighborhoodId { get; set; }
        public Neighborhood Neighborhood { get; set; }
    }
}
