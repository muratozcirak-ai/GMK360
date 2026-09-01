using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    public class Reservation : BaseEntity
    {
        public int UnitId { get; set; }
        public BuildingUnit Unit { get; set; }

        public string TenantUserId { get; set; }
        public Identity.ApplicationUser TenantUser { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public decimal TotalPrice { get; set; }
        public int GuestCount { get; set; }
        
        public ReservationStatus ReservationStatus { get; set; } = ReservationStatus.Pending;

        public virtual ICollection<GuestIdentity> Guests { get; set; } = new List<GuestIdentity>();
    }

    public enum ReservationStatus
    {
        Pending = 1,
        Confirmed = 2,
        CheckIn = 3,
        CheckOut = 4,
        Cancelled = 5
    }
}
