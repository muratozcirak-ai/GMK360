using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public enum WalletTransactionStatus
    {
        Pending = 1,   // Beklemede
        Cleared = 2,   // Onayland/Kullanlabilir
        Cancelled = 3  // ptal
    }

    public class WalletTransaction : BaseEntity
    {
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // Brut veya varsaylan miktar (Eski yapp bozulmasn diye decimal yapld)

        // YEN KURAL: Net Bakiye ve Stopaj
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } // Kesilen Stopaj/Vergi

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; } // Kullanlabilir Net Bakiye (Ekranda gzkmesi gereken)

        public WalletTransactionStatus Status { get; set; } = WalletTransactionStatus.Cleared;

        public DateTime? MaturityDate { get; set; } // Vade Tarihi (Referans kazanc iin r: 2 ay sonras)

        [Required]
        [MaxLength(200)]
        public string TransactionType { get; set; } = null!; // -rn: "Referans Geliri", "Aidat -demesi"
    }
}
