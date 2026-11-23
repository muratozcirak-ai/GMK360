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

        // Ödeme & Kampanya Takibi (SuperAdmin SaaS Yönetimi)
        public decimal PricePaid { get; set; }
        public int? DiscountPolicyId { get; set; }
        public virtual GMK360.Core.Entities.Marketing.DiscountPolicy? DiscountPolicy { get; set; }
        public string PaymentStatus { get; set; } = "Trial"; // Trial (15 Gün Deneme), Paid, PendingPayment, Expired, Exempt
        public DateTime? LastPaymentDate { get; set; }
        public string? AdminNotes { get; set; }
    }
}
