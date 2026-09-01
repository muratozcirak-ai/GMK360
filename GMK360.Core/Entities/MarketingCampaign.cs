using System;
using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities
{
    public class MarketingCampaign : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; } // Kampanya Şartları ve Detayı

        [Required]
        [MaxLength(500)]
        public string BannerImageUrl { get; set; } = null!; // Kampanya Afişi

        // Hedef Kitle (Örn: "Individual", "Estate", "Tradesmen", "Construction", "DailyRent", "RealEstateAgent", "All")
        [Required]
        [MaxLength(50)]
        public string TargetSegment { get; set; } = "All";

        [MaxLength(500)]
        public string? ActionUrl { get; set; } // "Hemen Üye Ol" butonunun gideceği link

        public bool IsActive { get; set; } = true;

        public DateTime? ValidUntil { get; set; } // Kampanya Bitiş Tarihi
    }
}
