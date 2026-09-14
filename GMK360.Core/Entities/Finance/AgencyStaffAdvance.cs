using System;

namespace GMK360.Core.Entities.Finance
{
    public enum AdvanceStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Paid = 3 // Avans personele ödendi (Hesaba yatırıldı)
    }

    public class AgencyStaffAdvance : BaseEntity
    {
        public int AgencyConsultantId { get; set; }
        public AgencyConsultant Consultant { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public string? Description { get; set; }

        public AdvanceStatus Status { get; set; } = AdvanceStatus.Pending;
        public DateTime? StatusDate { get; set; }

        // Avans, hangi ayın bordrosundan (maaşından) kesildi?
        public int? DeductedFromPayrollId { get; set; }
        public AgencyStaffPayroll? DeductedFromPayroll { get; set; }
    }
}
