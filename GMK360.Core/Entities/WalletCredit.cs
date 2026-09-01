using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class WalletCredit : BaseEntity
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public decimal Amount { get; set; }
        public string Source { get; set; } // "Referral", "Campaign" vs.
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; } = false;
        
        // Hediye bakiye süresi dolmuşsa geçerli değil
        public bool IsValid => !IsUsed && ExpiryDate >= DateTime.UtcNow;
    }
}
