using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class AgentSubscription : BaseEntity
    {
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int SubscriptionPackageId { get; set; }
        public SubscriptionPackage SubscriptionPackage { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public bool IsActive => DateTime.UtcNow <= EndDate && !IsDeleted;
    }
}
