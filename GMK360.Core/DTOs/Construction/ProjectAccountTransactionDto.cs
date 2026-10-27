using System;

namespace GMK360.Core.DTOs.Construction
{
    public class ProjectAccountTransactionDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public int ProjectId { get; set; }
        public int BankAccountId { get; set; }
        public string AccountName { get; set; } = "";
        public string TransactionType { get; set; } = "Giris"; // "Giris" (Para Girişi) veya "Cikis" (Para Çıkışı)
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "TRY";
        public decimal? ExchangeRate { get; set; } // Kur (USD, EUR vb. için)
        public decimal? AmountTL { get; set; } // TL Karşılığı
        public string AccountKind { get; set; } = "Resmi"; // "Resmi" veya "Elden"
        public int VatRate { get; set; } = 0; // %0, %1, %10, %20
        public decimal? VatAmount { get; set; }
        public decimal? NetAmount { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string CategoryOrPurpose { get; set; } = ""; // Ne İçin: Şantiye İaşesi/Yemek, Taşeron, Malzeme, vb.
        public string PartyName { get; set; } = ""; // Kim Yatırdı / Kime Ödendi
        public string? PaymentMethod { get; set; } = "IBAN"; // "IBAN" veya "Elden"
        public string? RecipientIban { get; set; } // Alıcı / Hak Sahibi IBAN'ı
        public string? RecipientBank { get; set; } // Alıcı Banka Adı
        public int? TargetBankAccountId { get; set; }
        public string? TargetAccountName { get; set; }
        public string? Description { get; set; }
        public string? ReceiptDocumentPath { get; set; }
        public string? ReceiptDocumentName { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
