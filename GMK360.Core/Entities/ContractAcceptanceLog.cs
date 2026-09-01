using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class ContractAcceptanceLog : BaseEntity
    {
        [Required]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public int ContractId { get; set; }
        
        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; }

        public int AcceptedVersion { get; set; } // Hangi versiyonu onayladı?

        public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(50)]
        public string IpAddress { get; set; } // Hangi IP'den onaylandı (Yasal zorunluluk için)
    }
}
