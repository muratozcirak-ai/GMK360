using System;
using System.Collections.Generic;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities.Marketplace
{
    public class MarketplaceJob : BaseEntity
    {
        public string Title { get; set; } // Örn: "120m2 Ev Boyama İşi"
        public string Description { get; set; } // "Duvarlar hazır, sadece işçilik aranıyor..."
        public string Location { get; set; } // "İstanbul / Kadıköy"
        
        public string JobType { get; set; } // "Boya", "Tesisat", "Fayans", "Genel Tadilat"
        public decimal? EstimatedBudget { get; set; }
        
        public string CreatedByUserId { get; set; }
        public ApplicationUser CreatedByUser { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<MarketplaceBid> Bids { get; set; } = new List<MarketplaceBid>();
    }
}

