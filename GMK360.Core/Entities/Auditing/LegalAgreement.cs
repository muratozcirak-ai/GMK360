using System;
using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities
{
    public enum AgreementType
    {
        UserAgreement = 1,
        Kvkk = 2,
        SmsUsage = 3,
        ReferralAndTax = 4,
        Other = 99
    }

    public class LegalAgreement : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } // Örn: "KVKK Aydınlatma Metni"

        [Required]
        public string Content { get; set; } // HTML sözleşme metni

        public AgreementType Type { get; set; }

        [Required]
        [MaxLength(20)]
        public string Version { get; set; } // Örn: "1.0", "1.1"

        public bool IsActive { get; set; } = true;
        public bool IsRequired { get; set; } = true; // Onaylanması zorunlu mu?
    }
}
