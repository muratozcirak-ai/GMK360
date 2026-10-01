using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public enum JobBidStatus
    {
        Pending = 1,
        Accepted = 2,
        Rejected = 3
    }

    public class JobBid : BaseEntity
    {
        [Required]
        public int JobRequestId { get; set; }
        public virtual JobRequest JobRequest { get; set; }

        [Required]
        public string ProviderUserId { get; set; } // Teklif veren Usta/TaYeron/Firma
        [ForeignKey("ProviderUserId")]
        public virtual ApplicationUser ProviderUser { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // Teklif edilen tutar

        public string Note { get; set; } // Ustann aYklamas (-rn: Malzeme dahil)

        public JobBidStatus Status { get; set; } = JobBidStatus.Pending;
    }
}
