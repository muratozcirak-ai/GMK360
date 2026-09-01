using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class SurveyOption : BaseEntity
    {
        public int MeetingSurveyId { get; set; }
        [ForeignKey("MeetingSurveyId")]
        public virtual MeetingSurvey MeetingSurvey { get; set; }

        [Required]
        [MaxLength(100)]
        public string OptionText { get; set; } // Örn: Evet, Hayır, Belki

        public virtual ICollection<SurveyVote> Votes { get; set; }
    }
}
