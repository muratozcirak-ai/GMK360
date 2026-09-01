using System;

namespace GMK360.Core.Entities
{
    public class GuestIdentity : BaseEntity
    {
        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        public string TCIdentityNumberOrPassport { get; set; }
        public DateTime DateOfBirth { get; set; }
        
        // Emniyet KBS bildirimi statüsü
        public bool IsReportedToKbs { get; set; } = false;
        public DateTime? KbsReportDate { get; set; }
    }
}
