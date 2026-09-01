using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class UserSubscription : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        public int PackageId { get; set; }
        public virtual SubscriptionPackage Package { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } // Null ise ömür boyu (Lifetime)

        public bool IsActive { get; set; } = true;

        // Ödeme takibi için (Opsiyonel, ayrı tabloda da tutulabilir)
        public decimal PricePaid { get; set; }
    }
}
