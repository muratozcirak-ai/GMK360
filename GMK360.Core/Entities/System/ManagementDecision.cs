using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class ManagementDecision : BaseEntity
    {
        public int? BuildingId { get; set; }
        [ForeignKey("BuildingId")]
        public virtual Building Building { get; set; }

        public int? HousingComplexId { get; set; }
        [ForeignKey("HousingComplexId")]
        public virtual HousingComplex HousingComplex { get; set; }

        [Required]
        public DateTime DecisionDate { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } // Örn: "Çatı Yapımı Kararı"

        [Required]
        public string Description { get; set; } // Karar metni

        public int? DocumentImageId { get; set; } // Orijinal karar defteri fotoğrafı / PDF (SystemDocument)
        // [ForeignKey("DocumentImageId")]
        // public virtual SystemDocument DocumentImage { get; set; }

        public bool IsPublished { get; set; } = true; // Sakinler görebilsin mi?
    }
}
