using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Construction;

namespace GMK360.Core.Entities.B2b
{
    public class B2BNetworkConnection : BaseEntity
    {
        [Required]
        public int AgencyId { get; set; }
        
        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }

        [Required]
        public int B2bCompanyId { get; set; }
        
        [ForeignKey("B2bCompanyId")]
        public B2bCompany B2bCompany { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? ConnectionNotes { get; set; }
    }
}