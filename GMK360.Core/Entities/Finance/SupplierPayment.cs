using System;

namespace GMK360.Core.Entities.Finance
{
    public enum PaymentMethod
    {
        Cash,           // Nakit
        BankTransfer,   // Havale/EFT
        CreditCard,     // Kredi Kartı
        Check,          // Çek
        PromissoryNote, // Senet
        OpenAccount     // Açık Hesap (Sadece Cari'ye atılan, ödemesiz kayıtlar için - Opsiyonel)
    }

    public enum PaymentStatus
    {
        Completed,      // Ödendi / Temizlendi
        Pending,        // Bekliyor (Örn: İleri tarihli çek)
        Bounced,        // Karşılıksız çıktı (Çek için)
        Cancelled       // İptal Edildi
    }

    public class SupplierPayment : BaseEntity
    {
        public int SupplierCurrentAccountId { get; set; }
        public SupplierCurrentAccount SupplierCurrentAccount { get; set; }

        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;

        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        
        // Sadece Çek/Senet için
        public DateTime? DueDate { get; set; } // Vade Tarihi
        public string? CheckNumber { get; set; } // Çek / Dekont Numarası
        public string? BankName { get; set; } // Hangi bankanın çeki/havalesi

        public string Notes { get; set; }
        public string HandledByUserId { get; set; }
    }
}
