using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities.Construction
{
    public class DocumentTemplate : BaseEntity
    {
        public int AgencyId { get; set; }
        [ForeignKey("AgencyId")]
        public virtual GMK360.Core.Entities.Agency Agency { get; set; }

        public string TemplateName { get; set; } 
        public string HtmlContent { get; set; } 
        public string Category { get; set; } 
    }
}
