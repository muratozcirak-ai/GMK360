using System;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Finance;

namespace GMK360.Core.Entities.Finance
{
    public class SubcontractorHakedis : BaseEntity
    {
        public int ContractId { get; set; }
        public virtual SubcontractorContract Contract { get; set; }

        public int HakedisNo { get; set; } 
        public DateTime HakedisDate { get; set; } = DateTime.Now;

        public string Description { get; set; } 

        // FİNANSAL ALANLAR
        public decimal ClaimAmount { get; set; } 
        public decimal DeductionAmount { get; set; } 
        public string? DeductionReason { get; set; } 

        public bool IsApproved { get; set; } = false;
    }
}
