using System.Collections.Generic;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class CrmContact : BaseEntity
    {
        // Kime ait (Emlakçı/Danışman)
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        public string? ContactType { get; set; } // Alici, Satici, Kiraci, EvSahibi

        public string? Notes { get; set; }

        // İlişkiler
        public ICollection<CrmDemand> Demands { get; set; }
        public ICollection<CrmAppointment> Appointments { get; set; }
        
        // Kiracı veya Ev Sahibi olarak yer aldığı sözleşmeler
        public ICollection<CrmRentalTracking> TenantContracts { get; set; }
        public ICollection<CrmRentalTracking> LandlordContracts { get; set; }
    }
}
