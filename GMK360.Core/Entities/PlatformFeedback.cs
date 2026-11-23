using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    /// <summary>
    /// Pilot / canlı kullanıcıların Site Yönetimi ve İnşaat Yönetimi dahil tüm modüllerden
    /// tek tıkla Süper Admin'e ilettiği geri bildirim, hata ve öneri kayıtları.
    /// </summary>
    public class PlatformFeedback : BaseEntity
    {
        [MaxLength(60)]
        public string ModuleArea { get; set; } = "Genel"; // SiteYonetimi, InsaatYonetimi, SakinPortal, Genel

        [MaxLength(40)]
        public string FeedbackType { get; set; } = "Oneri"; // Hata, Oneri, Soru, Degerlendirme

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? PageUrl { get; set; }

        [MaxLength(300)]
        public string? DeviceInfo { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [MaxLength(150)]
        public string? UserFullName { get; set; }

        [MaxLength(200)]
        public string? UserEmail { get; set; }

        [MaxLength(100)]
        public string? UserRoleLabel { get; set; }

        public int? ReferenceEntityId { get; set; }

        [MaxLength(30)]
        public string Status { get; set; } = "Yeni"; // Yeni, Inceleniyor, Cozuldu, Kapandi

        [MaxLength(2000)]
        public string? AdminNote { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }
}
