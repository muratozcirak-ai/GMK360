using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class BuildingStaffDocument : BaseEntity
    {
        public int BuildingStaffId { get; set; }

        [ForeignKey("BuildingStaffId")]
        public virtual BuildingStaff Staff { get; set; } = null!;

        [Required]
        [MaxLength(60)]
        public string DocumentType { get; set; } = "Diger";

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? FileUrl { get; set; }

        [MaxLength(250)]
        public string? PhysicalArchiveLocation { get; set; }

        public DateTime? IssueDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool IsVerified { get; set; } = true;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(450)]
        public string? UploadedByUserId { get; set; }

        [NotMapped]
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value.Date < DateTime.Today;

        /// <summary>
        /// Tesis kadrosu özlük dosyası için standart belge türleri ve zorunluluk durumu.
        /// </summary>
        public static readonly IReadOnlyList<(string Code, string Label, bool IsRequired)> StandardCatalog = new[]
        {
            ("KimlikFotokopisi", "Nüfus Cüzdanı / T.C. Kimlik Fotokopisi", true),
            ("Ikametgah", "İkametgah (Yerleşim Yeri) Belgesi", true),
            ("AdliSicil", "Adli Sicil (Sabıka) Kaydı", true),
            ("SaglikRaporu", "İşe Giriş Sağlık / Akciğer Raporu", true),
            ("SgkIseGiris", "SGK İşe Giriş Bildirgesi", true),
            ("IsSozlesmesi", "İmzalı Belirsiz/Belirli Süreli İş Sözleşmesi", true),
            ("VesikalikFoto", "Vesikalık Fotoğraf (2 Adet)", false),
            ("AileDurum", "Aile Durum & AGİ / IBAN Bildirimi", false),
            ("Diger", "Diğer Özlük / Sertifika Evrakı", false)
        };

        public static string GetLabel(string? code)
        {
            foreach (var item in StandardCatalog)
            {
                if (string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase))
                    return item.Label;
            }
            return string.IsNullOrWhiteSpace(code) ? "Özlük Belgesi" : code;
        }
    }
}
