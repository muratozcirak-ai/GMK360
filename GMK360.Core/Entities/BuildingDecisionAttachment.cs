using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class BuildingDecisionAttachment : BaseEntity
    {
        public int SystemMeetingId { get; set; }
        [ForeignKey("SystemMeetingId")]
        public virtual SystemMeeting SystemMeeting { get; set; } = null!;

        public int? BuildingUnitId { get; set; }

        [MaxLength(450)]
        public string? AuthorUserId { get; set; }

        [Required, MaxLength(150)]
        public string AuthorName { get; set; } = string.Empty;

        [MaxLength(40)]
        public string AuthorRoleLabel { get; set; } = "Kat Maliki"; // "Yönetici", "Kat Maliki", "Vekil", "Kiracı"

        [MaxLength(2000)]
        public string? CommentText { get; set; }

        [MaxLength(200)]
        public string? DocumentTitle { get; set; }

        [MaxLength(500)]
        public string? FileUrl { get; set; }
    }
}
