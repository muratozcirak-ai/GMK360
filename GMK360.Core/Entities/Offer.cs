using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class Offer : BaseEntity
    {
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public string BuyerId { get; set; }
        public ApplicationUser Buyer { get; set; }

        public string SellerId { get; set; }
        public ApplicationUser Seller { get; set; }

        public decimal OfferedPrice { get; set; }

        public OfferStatus Status { get; set; }
    }

    public enum OfferStatus
    {
        Pending,
        Accepted,
        Rejected,
        CounterOffer
    }
}
