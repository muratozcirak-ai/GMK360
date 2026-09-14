using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Finance
{
    public class AgencyStaffPayroll : BaseEntity
    {
        public int AgencyConsultantId { get; set; }
        public AgencyConsultant Consultant { get; set; }

        public string Period { get; set; } // Örn: 2026-08
        public DateTime GenerationDate { get; set; } = DateTime.UtcNow;

        public decimal BaseSalary { get; set; } // O ayki kök maaş
        public decimal BonusAmount { get; set; } = 0; // Prim, Mesai vb
        public decimal DeductionAmount { get; set; } = 0; // Toplam kesinti (Avanslar vb)

        // Ele geçecek net tutar (Base + Bonus - Deduction)
        public decimal NetPayableAmount { get; set; }

        public bool IsPaid { get; set; } = false;
        public DateTime? PaymentDate { get; set; }

        public string? Notes { get; set; }

        // Bu maaştan kesilen avanslar
        public ICollection<AgencyStaffAdvance>? DeductedAdvances { get; set; }
    }
}
