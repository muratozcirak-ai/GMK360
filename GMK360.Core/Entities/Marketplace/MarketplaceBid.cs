using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities.Marketplace
{
    public class MarketplaceBid : BaseEntity
    {
        public int MarketplaceJobId { get; set; }
        public MarketplaceJob MarketplaceJob { get; set; }

        public string BidderUserId { get; set; }
        public ApplicationUser BidderUser { get; set; }

        public decimal OfferedPrice { get; set; }
        public string Message { get; set; } // "Malzeme benden 50.000 TL olur."
        
        public bool IsAccepted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
