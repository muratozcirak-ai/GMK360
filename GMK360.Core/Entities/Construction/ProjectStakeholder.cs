using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities.Construction
{
    public enum StakeholderRole
    {
        Landowner = 1,       // Arsa Sahibi / Hak Sahibi
        Representative = 2,  // Yasal Temsilci / Avukat
        Consultant = 3,      // Proje Takipçisi / Müşavir
        Other = 4
    }

    public class ProjectStakeholder : BaseEntity
    {
        [Required]
        public int ProjectId { get; set; }
        
        [ForeignKey("ProjectId")]
        public ConstructionProject Project { get; set; }

        [Required]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public StakeholderRole Role { get; set; } = StakeholderRole.Landowner;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? SharePercentage { get; set; } // Sadece Arsa Sahibi için

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}