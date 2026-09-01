using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class SystemMeetingDecision : BaseEntity
    {
        public int SystemMeetingId { get; set; }
        [ForeignKey("SystemMeetingId")]
        public virtual SystemMeeting SystemMeeting { get; set; }

        [Required]
        public string DecisionText { get; set; }
    }
}
