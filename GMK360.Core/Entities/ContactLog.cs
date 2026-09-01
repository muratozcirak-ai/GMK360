using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class ContactLog : BaseEntity
    {
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public string? ConsultantId { get; set; } // Emlakçı (Danışman)
        public ApplicationUser Consultant { get; set; }

        public int? ServiceProviderId { get; set; } // Usta/Esnaf
        public ServiceProvider ServiceProvider { get; set; }

        public string? UserId { get; set; } // Müşteri (Üye ise)
        public ApplicationUser User { get; set; }

        public string? GuestEmail { get; set; } // Üye değilse
        public string? GuestPhone { get; set; } // Üye değilse

        public DateTime ContactDate { get; set; } = DateTime.UtcNow;
        public bool HasRated { get; set; } = false; // Müşteri bu iletişimden sonra puan verdi mi?
    }
}
