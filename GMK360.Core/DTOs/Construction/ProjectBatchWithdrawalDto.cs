using System;
using System.Collections.Generic;

namespace GMK360.Core.DTOs.Construction
{
    public class BatchWithdrawalItemDto
    {
        public DateTime? TransactionDate { get; set; }
        public string RecipientName { get; set; } = "";
        public string OutflowCategory { get; set; } = "";
        public int? BankAccountId { get; set; }
        public string AccountKind { get; set; } = "Resmi"; // "Resmi" veya "Elden" (Hesaptan otomatik algılanır)
        public string PaymentMethod { get; set; } = "IBAN"; // "IBAN" veya "Elden"
        public string? RecipientIban { get; set; }
        public string? RecipientBank { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "TRY";
        public int VatRate { get; set; } = 0;
        public string VatType { get; set; } = "dahil"; // "dahil" veya "haric"
        public string? Notes { get; set; }
    }

    public class ProjectBatchWithdrawalRequestDto
    {
        public int ProjectId { get; set; }
        public int? SourceBankAccountId { get; set; }
        public List<BatchWithdrawalItemDto> Items { get; set; } = new();
    }
}
