using System;
using System.Collections.Generic;

namespace GMK360.Core.DTOs.Construction
{
    public class BatchDepositItemDto
    {
        public DateTime? TransactionDate { get; set; }
        public string SourceName { get; set; } = ""; // Kimden / Muhatap
        public string InflowCategory { get; set; } = ""; // Giriş Türü (Ortak Sermayesi, Daire Satışı, vb.)
        public int? BankAccountId { get; set; } // Hangi Kasaya / Hesaba
        public string AccountKind { get; set; } = "Resmi"; // Hesaptan otomatik algılanır
        public string PaymentMethod { get; set; } = "IBAN"; // IBAN veya Elden
        public string? SenderIban { get; set; }
        public string? SenderBank { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "TRY";
        public int VatRate { get; set; } = 0; // KDV Oranı (%0, %1, %10, %20)
        public string VatType { get; set; } = "dahil"; // "dahil" veya "haric"
        public string? Notes { get; set; }
    }

    public class ProjectBatchDepositRequestDto
    {
        public int ProjectId { get; set; }
        public int? TargetBankAccountId { get; set; }
        public List<BatchDepositItemDto> Items { get; set; } = new();
    }
}
