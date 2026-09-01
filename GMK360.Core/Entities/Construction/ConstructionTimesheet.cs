using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities.Construction
{
    public class ConstructionTimesheet : BaseEntity
    {
        [Required]
        public int ConstructionProjectId { get; set; }
        public ConstructionProject ConstructionProject { get; set; }

        [Required]
        public string WorkerId { get; set; } // Usta / İşçi
        public ApplicationUser Worker { get; set; }

        [Required]
        public string RecordedById { get; set; } // Şantiye Şefi
        public ApplicationUser RecordedBy { get; set; }

        [Required]
        public DateTime WorkDate { get; set; }

        public string ShiftType { get; set; } = "Tam Yevmiye"; // Tam Yevmiye, Yarım Yevmiye, Mesai
        
        public decimal? Hours { get; set; } // Eğer saatlik mesai ise

        // 0 = Bekliyor, 1 = Onaylandı, 2 = Reddedildi
        public int ApprovalStatus { get; set; } = 0;

        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }

        [MaxLength(100)]
        public string PwaAccessToken { get; set; } // SMS ile giden token
    }
}
