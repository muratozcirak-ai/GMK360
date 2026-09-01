using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class PropertyReservation : BaseEntity
    {
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public string GuestUserId { get; set; } // Nullable, Airbnb vb. dış kaynaklardan gelenler için
        public ApplicationUser GuestUser { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        
        public string Status { get; set; } // Pending, Confirmed, BlockedByHost

        // Finansal Takip
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentStatus { get; set; } // Bekliyor, Kapora Alındı, Tamamlandı

        // KBS Emniyet Bildirimi
        public bool IsKbsReported { get; set; }
        public string KbsErrorMessage { get; set; }

        // İlişkili Misafirler (Çoklu Misafir Desteği)
        public System.Collections.Generic.ICollection<GuestCheckInRecord> Guests { get; set; }

        // Komisyon / Partner Takibi
        public int? PartnerId { get; set; }
        public Partner Partner { get; set; }
        
        public decimal CommissionAmount { get; set; } // KomisyonHakEdisi
        public bool IsCommissionPaid { get; set; } // KomisyonOdendiMi
        public string CommissionDocumentType { get; set; } // KomisyonBelgeTipi
        public DateTime? CommissionPaymentDate { get; set; } // KomisyonOdemeTarihi
    }
}
