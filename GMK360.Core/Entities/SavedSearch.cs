using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class SavedSearch : BaseEntity
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string SearchName { get; set; } // Örn: "Kadıköy Site İçi Aramam"

        // Çoklu seçim verileri JSON Array olarak tutulabilir: "[1, 2, 5]" veya virgülle ayrılmış "1,2,5"
        public string CityIds { get; set; } 
        public string DistrictIds { get; set; }
        public string NeighborhoodIds { get; set; }
        public string CategoryIds { get; set; } // SubType ID'leri

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Müşterinin spesifik özellik filtreleri (Havuzlu, Eşyalı vb.) 
        public string FeaturesJSON { get; set; }
        
        public bool IsEmailNotificationEnabled { get; set; } = true;
        
        public string SearchCriteriaJson { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastNotifiedAt { get; set; }
    }
}
