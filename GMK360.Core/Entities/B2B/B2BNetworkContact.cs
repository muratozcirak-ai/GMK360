using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities.B2B
{
    // Firmaların (Ajansların) rehberindeki gölge veya gerçek taşeron/tedarikçiler
    public class B2BNetworkContact : BaseEntity
    {
        // Kök: Bu kontağı kim oluşturdu / kimin referansıyla geldi?
        public int OwnerAgencyId { get; set; }
        public Agency OwnerAgency { get; set; }

        public string AddedByUserId { get; set; } // Ekleyen personel
        
        // Firma/Usta Bilgileri
        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        
        // Hangi alanda hizmet veriyor? (Örn: Betoncu, Çatıcı, Demirci)
        public string SectorCategory { get; set; } 

        // Eğer sistemimize kendi isteğiyle veya SMS linkiyle üye olursa, gerçek ID'si buraya işlenir
        public int? RegisteredAgencyId { get; set; }
        public Agency RegisteredAgency { get; set; }

        public bool IsRegistered => RegisteredAgencyId.HasValue;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
