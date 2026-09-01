using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class SystemDocument : BaseEntity
    {
        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; } // S3 veya sunucu fiziksel yolu

        [MaxLength(255)]
        public string FileName { get; set; } // Örn: elektrik_faturasi_nisan.pdf

        /// <summary>
        /// 1 = Gider/Fatura
        /// 2 = Usta Sözleşmesi
        /// 3 = Aidat Dekontu
        /// 4 = Tadilat Öncesi Fotoğraf
        /// </summary>
        [Required]
        public int ModuleType { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReferenceId { get; set; } // Hangi tablo kaydına ait (Polymorphic Guid/Int)

        public string UploadedById { get; set; }
        [ForeignKey("UploadedById")]
        public virtual ApplicationUser UploadedBy { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    }
}
