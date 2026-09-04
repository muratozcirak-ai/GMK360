using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities.Construction
{
    public class ProjectAssignment : BaseEntity
    {
        [Required]
        public int ConstructionProjectId { get; set; }
        public ConstructionProject ConstructionProject { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [MaxLength(100)]
        public string? RoleInProject { get; set; } // Örn: Şantiye Şefi, Saha Mühendisi
        
        public bool IsActive { get; set; } = true;
    }
}
