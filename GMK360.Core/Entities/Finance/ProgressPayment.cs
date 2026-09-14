using System;

namespace GMK360.Core.Entities.Finance
{
    public enum ProgressPaymentStatus
    {
        Draft = 0,
        PendingApproval = 1,
        Approved = 2,
        Paid = 3,
        Rejected = 4
    }

    public class ProgressPayment : BaseEntity
    {
        public int SubcontractorContractId { get; set; }
        public SubcontractorContract Contract { get; set; }

        public int? ContractPhaseId { get; set; }
        public ContractPhase Phase { get; set; }

        public string PaymentTitle { get; set; } // Örn: 'A Blok Temel Hakediþi No:1'
        public string? Notes { get; set; }
        
        public decimal RequestedAmount { get; set; } // Taþeronun talep ettiði
        public decimal ApprovedAmount { get; set; } // Müteahhidin onayladýðý
        public decimal RetentionAmount { get; set; } = 0; // Kesinti (Teminat / Stopaj)

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovalDate { get; set; }

        public ProgressPaymentStatus Status { get; set; } = ProgressPaymentStatus.Draft;
    }
}

