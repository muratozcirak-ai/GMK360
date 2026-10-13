using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class DnaTemplateOption
    {
        [Key]
        public int Id { get; set; }

        public int DnaTemplateQuestionId { get; set; }
        [ForeignKey("DnaTemplateQuestionId")]
        public virtual DnaTemplateQuestion Question { get; set; }

        [Required]
        [MaxLength(200)]
        public string OptionText { get; set; }

        public int? DnaProfessionId { get; set; }
        [ForeignKey("DnaProfessionId")]
        public virtual DnaProfession Profession { get; set; }

        public int OptionOrder { get; set; }
    }
}