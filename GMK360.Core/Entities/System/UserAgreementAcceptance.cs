using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class UserAgreementAcceptance : BaseEntity
    {
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public int LegalAgreementId { get; set; }
        [ForeignKey("LegalAgreementId")]
        public virtual LegalAgreement LegalAgreement { get; set; }

        public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string IpAddress { get; set; } // Yasal ispat için
    }
}
