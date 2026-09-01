using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class Article : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(250)]
        public string Slug { get; set; } = null!; // SEO uyumlu URL (örn: cati-masrafi-vergiden-nasil-dusulur)

        [Required]
        public string HtmlContent { get; set; } = null!; // WYSIWYG editor içeriği

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        [MaxLength(200)]
        public string? SeoTags { get; set; } // Virgülle ayrılmış anahtar kelimeler

        public string Category { get; set; } = "Genel";
        
        public bool IsMembersOnly { get; set; } = false; // Sadece giriş yapanlara mı özel?
        
        public bool IsPublished { get; set; } = false;

        public DateTime? PublishedAt { get; set; }

        // --- YAZAR / DANIŞMAN BİLGİSİ (Win-Win Modeli) ---
        public string? AuthorUserId { get; set; }
        
        [ForeignKey("AuthorUserId")]
        public virtual ApplicationUser? Author { get; set; }
    }
}
