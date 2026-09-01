using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class ReferenceLog : BaseEntity
    {
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!; // Referansı gösteren kişi (Usta vs)

        [Required]
        [MaxLength(20)]
        public string TargetPhoneNumber { get; set; } = null!; // Onay mesajı giden numara

        [MaxLength(50)]
        public string? IpAddress { get; set; } // Onayı veren kişinin IP adresi (Webhook'tan veya tıklamadan alınır)

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        
        public DateTime? ApprovedAt { get; set; }
    }
}
