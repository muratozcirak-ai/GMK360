using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities.Construction
{
    public class ProjectAmenity : BaseEntity
    {
        [Required]
        public int ConstructionProjectId { get; set; }
        public ConstructionProject ConstructionProject { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Örn: Açık Otopark, Süs Havuzu, Kamelya

        [MaxLength(50)]
        public string? Type { get; set; } // Örn: Sosyal Tesis, Peyzaj, Spor Alanı

        public double? SquareMeters { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsCompleted { get; set; } = false;
    }
}
