using System;

namespace GMK360.Core.Entities
{
    public class DmsDocument : BaseEntity
    {
        public int FolderId { get; set; }
        public DmsFolder Folder { get; set; }

        public string Title { get; set; } // Belgenin Adı: "Ocak Ayı Faturası", "Kira Sözleşmesi"
        public string DocumentUrl { get; set; } // Fiziksel dosyanın sunucudaki konumu (PDF, JPG)
        public string FileExtension { get; set; } // .pdf, .jpg
        public long FileSizeBytes { get; set; }

        // Fiziksel Arşiv Takibi (Orjinali nerede?)
        public string PhysicalLocationNote { get; set; } // Örn: "Orijinali Merkez Ofis, Mavi Klasör No:4, Şeffaf Dosya içinde"
        public bool IsOriginalRequired { get; set; } // İleride ıslak imza veya orjinali lazım mı?

        // Polymorphic Ownership (Aidiyet Takibi)
        // Bu belge sistemde hangi veriye ait? (Property, Building, Tenant, ExpenseRecord)
        public string EntityType { get; set; } 
        public int? EntityId { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;
        public string UploadedByUserId { get; set; }
    }
}
