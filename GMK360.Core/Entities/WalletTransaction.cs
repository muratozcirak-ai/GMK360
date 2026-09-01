using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class WalletTransaction : BaseEntity
    {
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        public int Amount { get; set; } // Eksi (harcama) veya Artı (yükleme) jeton miktarı

        [Required]
        [MaxLength(200)]
        public string TransactionType { get; set; } = null!; // Örn: "AI Değerleme Raporu", "Bakiye Yükleme", "Bana Emlakçı Bul"
    }
}
