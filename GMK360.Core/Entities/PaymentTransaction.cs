using System;

namespace GMK360.Core.Entities
{
    public enum PaymentContextType
    {
        Rent,              // Kira
        Dues,              // Aidat
        Maintenance,       // Usta/Tadilat İşleri
        Material,          // Nalbur/Esnaf
        ShortTermBooking,  // Günlük Kiralama
        BrokerageFee,      // Emlakçı Komisyonu
        Subscription       // Platform Aboneliği
    }

    public enum PaymentTransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public class PaymentTransaction : BaseEntity
    {
        public string SenderUserId { get; set; } // Ödemeyi Yapan
        public Identity.ApplicationUser SenderUser { get; set; }

        public int ReceiverAccountId { get; set; } // Paranın yatacağı evrensel hesap
        public FinancialAccount ReceiverAccount { get; set; }

        public decimal Amount { get; set; } // Toplam Çekilen Tutar
        public decimal PlatformCommissionAmount { get; set; } // Platformun Kestiği
        public decimal NetReceiverAmount { get; set; } // Alıcıya Gidecek Net Tutar

        public string TransactionId { get; set; } // İyzico'dan dönecek referans
        public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.Pending;

        public PaymentContextType ContextType { get; set; }
        public Guid? ReferenceId { get; set; } // Hangi Entity için? (Örn: UnitDebtId, ReservationId, vs. - gevşek bağ)
    }
}
