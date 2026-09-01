using GMK360.Core.Entities.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class SurveyVote : BaseEntity
    {
        public int SurveyOptionId { get; set; }
        [ForeignKey("SurveyOptionId")]
        public virtual SurveyOption SurveyOption { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
