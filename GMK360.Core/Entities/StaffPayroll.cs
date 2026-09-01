using System;

namespace GMK360.Core.Entities
{
    public class StaffPayroll : BaseEntity
    {
        public int ManagementMemberId { get; set; }
        public ManagementMember ManagementMember { get; set; }

        public string Period { get; set; } // Örn: 2026-08 (Yıl-Ay)
        public decimal NetSalary { get; set; }
        
        public bool IsPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        
        public int? PaidFromBankAccountId { get; set; } // Hangi hesaptan ödendi
        public BankAccount? PaidFromBankAccount { get; set; }
    }
}
