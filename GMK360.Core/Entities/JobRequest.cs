using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public enum JobRequestStatus
    {
        Open = 1,           // Ak / Teklif Bekliyor
        Reviewing = 2,      // Teklifler DeYerlendiriliyor
        Approved = 3,       // Bir teklif onayland, iY baYlad
        Completed = 4,      // Y bitti (Masrafa dnǬYtǬrǬlme aYamas)
        Cancelled = 5       // ptal edildi
    }

    public class JobRequest : BaseEntity
    {
        public int? PropertyId { get; set; } // Bireysel daire iYiyse
        public virtual Property Property { get; set; }

        public int? BuildingId { get; set; } // Ortak alan/bina iYiyse
        public virtual Building Building { get; set; }

        [Required]
        public string CreatorUserId { get; set; } // Talebi aan (Ynetici veya Ev Sahibi)
        [ForeignKey("CreatorUserId")]
        public virtual ApplicationUser CreatorUser { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } // -rn: "at Akyor, Yaltm Lazm"

        [Required]
        public string Description { get; set; }

        public JobRequestStatus Status { get; set; } = JobRequestStatus.Open;

        public virtual ICollection<JobBid> Bids { get; set; } = new List<JobBid>();
    }
}
