using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class DocumentArchive : BaseEntity
    {
        // Çok kiracılı yapı (Construction, RealEstate vs. paylaşımlı havuz)
        public int? AgencyId { get; set; }
        [ForeignKey("AgencyId")]
        public virtual GMK360.Core.Entities.Agency Agency { get; set; }

        public int? BuildingId { get; set; }
        public virtual Building Building { get; set; }

        public string Title { get; set; } // Asansör Raporu, SGK Bildirgesi vb.
        public string DocumentUrl { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;

        // --- YENİ EKLENEN ARŞİV VE SÖZLEŞME ALANLARI ---
        public string FileName { get; set; } 
        public string FileExtension { get; set; } 
        
        public string Category { get; set; } 
        public int? Year { get; set; } 
        
        public string SourceModule { get; set; } 
        public int? SourceRecordId { get; set; } 
        
        public int? ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public virtual GMK360.Core.Entities.Construction.ConstructionProject Project { get; set; }

        public string Status { get; set; } = "Tamamlandı"; // "Tamamlandı", "Islak İmza Bekliyor" vb.
    }
}
