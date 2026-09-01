using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class KbsFacilitySettings : BaseEntity
    {
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public string EgmFacilityCode { get; set; } // Emniyetten alınan tesis kodu
        public string EgmPasswordEncrypted { get; set; } // Şifrelenmiş parola

        public bool IsActive { get; set; } = true;
    }
}
