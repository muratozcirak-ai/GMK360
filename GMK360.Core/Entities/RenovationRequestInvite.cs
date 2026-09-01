using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class RenovationRequestInvite : BaseEntity
    {
        public int RenovationRequestId { get; set; }
        public RenovationRequest RenovationRequest { get; set; }

        public string ServiceProviderId { get; set; } // ApplicationUserId (Usta)
        public ApplicationUser ServiceProvider { get; set; }

        public bool HasResponded { get; set; } = false;
        public bool IsAccepted { get; set; } = false; // Teklif verdiyse true
    }
}
