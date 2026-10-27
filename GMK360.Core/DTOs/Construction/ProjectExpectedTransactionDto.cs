using System;

namespace GMK360.Core.DTOs.Construction
{
    public class ProjectExpectedTransactionDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public int ProjectId { get; set; }
        public string Direction { get; set; } = "Inflow"; // "Inflow" (Beklenen Giriş) veya "Outflow" (Beklenen Çıkış)
        public string PartyName { get; set; } = ""; // Kimden / Kime
        public string? PaymentMethod { get; set; } = "IBAN"; // "IBAN" veya "Elden"
        public string? RecipientIban { get; set; } // Alıcı / Hak Sahibi IBAN'ı
        public string? RecipientBank { get; set; } // Alıcı Banka Adı
        public string? RecipientPhone { get; set; } // Alıcı / Hak Sahibi Telefon
        public string? RecipientEmail { get; set; } // Alıcı / Hak Sahibi E-posta
        public DateTime Date1 { get; set; } = DateTime.Today; // Tarih 1
        public DateTime? Date2 { get; set; } // Tarih 2 (Opsiyonel / Aralık)
        public string AccountType { get; set; } = "Resmi"; // "Resmi" veya "Elden"
        public int? TargetBankAccountId { get; set; }
        public string? TargetAccountName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "TRY";
        public string CategoryOrPurpose { get; set; } = ""; // Ne İçin (Hak Sahipleri Kira Desteği, Hakediş, vb.)
        public string? Description { get; set; }
        public string? DocumentPath { get; set; }
        public bool IsCompleted { get; set; } = false;
        public string? CompletedTransactionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
