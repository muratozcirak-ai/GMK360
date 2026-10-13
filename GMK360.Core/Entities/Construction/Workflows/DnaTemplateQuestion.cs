using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class DnaTemplateQuestion
    {
        [Key]
        public int Id { get; set; }

        public int DnaTemplateStepId { get; set; }
        
        [ForeignKey("DnaTemplateStepId")]
        public virtual DnaTemplateStep Step { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public bool IsMultipleChoice { get; set; }

        public bool IsRequired { get; set; }

        public int? DnaProfessionId { get; set; }

        [ForeignKey("DnaProfessionId")]
        public virtual DnaProfession Profession { get; set; }

        [MaxLength(500)]
        public string OptionSource { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        public int QuestionOrder { get; set; }

        public virtual ICollection<DnaTemplateOption> Options { get; set; } = new List<DnaTemplateOption>();
    }
}