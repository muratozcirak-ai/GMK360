using System;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public enum EscrowStatus
    {
        Pending,   // Havuzda bekliyor
        Released,  // Hak sahiplerinin bakiyesine aktarıldı
        Refunded   // Alıcıya iade edildi
    }

    public class EscrowTransaction : BaseEntity
    {
        [ForeignKey("PaymentTransaction")]
        public int PaymentTransactionId { get; set; }
        public virtual PaymentTransaction PaymentTransaction { get; set; } = null!;

        public decimal TotalAmount { get; set; } // Toplam çekilen ana para

        // Platform Kesintisi
        public decimal PlatformCommissionGross { get; set; } // Bizim kestiğimiz toplam para

        // Satıcı / Hizmet Veren
        public string? SellerUserId { get; set; }
        public virtual ApplicationUser? SellerUser { get; set; }
        public decimal SellerGrossAmount { get; set; } 
        public decimal SellerTaxAmount { get; set; } // Stopaj vs.
        public decimal SellerNetAmount { get; set; } // Gerçekte hesabına geçecek para (Gross - Tax)

        // Referans Alan Kişi (Platform komisyonu üzerinden pay alır)
        public string? ReferrerUserId { get; set; }
        public virtual ApplicationUser? ReferrerUser { get; set; }
        public decimal ReferrerGrossAmount { get; set; }
        public decimal ReferrerTaxAmount { get; set; }
        public decimal ReferrerNetAmount { get; set; }

        public DateTime ReleaseDate { get; set; } // Paranın serbest kalacağı tarih
        
        public EscrowStatus Status { get; set; } = EscrowStatus.Pending;
    }
}
