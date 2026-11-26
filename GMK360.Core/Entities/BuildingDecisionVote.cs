using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class BuildingDecisionVote : BaseEntity
    {
        public int SystemMeetingId { get; set; }
        [ForeignKey("SystemMeetingId")]
        public virtual SystemMeeting SystemMeeting { get; set; } = null!;

        public int BuildingUnitId { get; set; }
        [ForeignKey("BuildingUnitId")]
        public virtual BuildingUnit BuildingUnit { get; set; } = null!;

        [MaxLength(450)]
        public string? VoterUserId { get; set; }

        [Required, MaxLength(150)]
        public string VoterName { get; set; } = string.Empty;

        [Required, MaxLength(25)]
        public string ParticipationType { get; set; } = "Asil"; // "Asil", "Vekil", "Katilmadi"

        [MaxLength(150)]
        public string? ProxyPersonName { get; set; }

        [MaxLength(500)]
        public string? ProxyDocumentUrl { get; set; }

        [Required, MaxLength(20)]
        public string VoteChoice { get; set; } = "Pending"; // "Pending", "Kabul", "Ret"

        [MaxLength(1000)]
        public string? DissentNote { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitLandShareSnapshot { get; set; }

        [MaxLength(40)]
        public string? ApprovalTraceCode { get; set; }

        public DateTime? VotedAt { get; set; }
    }
}
