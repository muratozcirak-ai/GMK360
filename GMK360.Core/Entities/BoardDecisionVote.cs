using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class BoardDecisionVote : BaseEntity
    {
        public int SystemMeetingId { get; set; }
        [ForeignKey("SystemMeetingId")]
        public virtual SystemMeeting SystemMeeting { get; set; } = null!;

        public int BuildingManagerId { get; set; }
        [ForeignKey("BuildingManagerId")]
        public virtual BuildingManager BuildingManager { get; set; } = null!;

        [MaxLength(450)]
        public string? VoterUserId { get; set; }

        [MaxLength(150)]
        public string VoterName { get; set; } = string.Empty;

        [MaxLength(80)]
        public string RoleTitle { get; set; } = string.Empty;

        public int HierarchyLevel { get; set; } = 3;

        [MaxLength(20)]
        public string VoteChoice { get; set; } = "Pending"; // Kabul, Ret, Pending

        [MaxLength(1000)]
        public string? DissentNote { get; set; }

        [MaxLength(40)]
        public string? ApprovalTraceCode { get; set; }

        public DateTime? VotedAt { get; set; }
    }
}
