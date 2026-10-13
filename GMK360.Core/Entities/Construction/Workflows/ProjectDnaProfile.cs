using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class ProjectDnaProfile
    {
        [Key]
        public int Id { get; set; }

        public int ProjectId { get; set; }
        
        [ForeignKey("ProjectId")]
        public virtual ConstructionProject Project { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; } // Örn: "Ultra Lüks Konfigürasyon"

        [MaxLength(50)]
        public string Status { get; set; } = "DXF Bekliyor";

        [MaxLength(100)]
        public string CreatedBy { get; set; } // Session/User ID veya Adı

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Yedek (Stepne) Kolonlar (Gelecek esnekliği için)
        [MaxLength(500)]
        public string? ExtraField1 { get; set; }

        [MaxLength(500)]
        public string? ExtraField2 { get; set; }

        // Child Tables
        public virtual ICollection<ProjectDnaDetail> Details { get; set; } = new List<ProjectDnaDetail>();
    }
}
