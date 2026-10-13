using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class DnaTemplateStep
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string StepName { get; set; } // Örn: Adım 0: Mecburi Statik

        public int StepOrder { get; set; } // Örn: 0, 1, 2

        public virtual ICollection<DnaTemplateQuestion> Questions { get; set; } = new List<DnaTemplateQuestion>();
    }
}