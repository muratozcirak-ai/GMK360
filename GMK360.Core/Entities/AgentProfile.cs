using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class AgentProfile : BaseEntity
    {
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public string CompanyName { get; set; }
        public string TaxNumber { get; set; }
        public string LicenseNumber { get; set; } // Taşınmaz Ticareti Yetki Belgesi Numarası
        
        public bool IsApprovedByAdmin { get; set; } // Admin Onayı
        public string EidsVerificationStatus { get; set; } // Bekliyor, Onaylandı, Reddedildi
    }
}
