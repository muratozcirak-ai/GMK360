using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class ProjectDnaDetail
    {
        [Key]
        public int Id { get; set; }

        public int ProfileId { get; set; }
        
        [ForeignKey("ProfileId")]
        public virtual ProjectDnaProfile Profile { get; set; }

        [Required]
        [MaxLength(100)]
        public string Phase { get; set; } // �rn: "Faz 1", "Ad�m 3"

        [Required]
        [MaxLength(250)]
        public string SelectedItem { get; set; } // �rn: "�ift Kat Membran"

        // ---- YEN� EKLENEN Z�NC�R BA�LANTISI (K�M YAPAR?) ----
        public int? DnaProfessionId { get; set; }
        [ForeignKey("DnaProfessionId")]
        public virtual DnaProfession Profession { get; set; }
        // -----------------------------------------------------

        // S�f�r kalacak (DXF bekleyen) alanlar
        public decimal Quantity { get; set; } = 0;

        [MaxLength(20)]
        public string Unit { get; set; } // �rn: m2, mt, Adet

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
