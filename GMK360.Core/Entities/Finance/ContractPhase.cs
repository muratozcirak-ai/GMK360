using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Finance
{
    public class ContractPhase : BaseEntity
    {
        public int SubcontractorContractId { get; set; }
        public SubcontractorContract Contract { get; set; }

        public string PhaseName { get; set; } // Örn: '%20 Yýkým Tamamlanmasý', 'Temel Atýlmasý'
        public string? Description { get; set; }
        
        public decimal Amount { get; set; } // Bu fazýn parasal karþýlýðý
        
        public DateTime? TargetDate { get; set; } // Planlanan tamamlama
        
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedDate { get; set; }

        public ICollection<ProgressPayment> ProgressPayments { get; set; }
    }
}
