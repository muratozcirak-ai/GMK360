using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class MeetingSurvey : BaseEntity
    {
        public int SystemMeetingId { get; set; }
        [ForeignKey("SystemMeetingId")]
        public virtual SystemMeeting SystemMeeting { get; set; }

        [Required]
        [MaxLength(200)]
        public string QuestionText { get; set; } // Örn: Çatı yalıtımı yapılsın mı?

        public bool IsActive { get; set; } = true; // Anket oylamaya açık mı?

        public virtual ICollection<SurveyOption> Options { get; set; }
    }
}
