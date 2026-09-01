using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public enum InvitationRole
    {
        Tenant,
        Owner,
        Tradesman,
        Manager
    }

    public class InvitationToken : BaseEntity
    {
        public Guid Token { get; set; } = Guid.NewGuid();
        
        public string TargetEmailOrPhone { get; set; }
        
        public string InviterUserId { get; set; }
        public ApplicationUser InviterUser { get; set; }

        public InvitationRole TargetRole { get; set; }
        
        // Davet edilen mülk veya iş vs. için (Opsiyonel)
        public int? RelatedUnitId { get; set; }
        public BuildingUnit RelatedUnit { get; set; }

        public DateTime ExpirationDate { get; set; }
        public bool IsUsed { get; set; } = false;
    }
}
