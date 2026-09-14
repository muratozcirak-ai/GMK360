using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public enum AgencyRole
    {
        Owner,        // Firma Sahibi (Tüm yetkiler)
        Manager,      // Yönetici (Yetkili)
        Consultant,   // Danışman (Kendi ilanları)
        Secretary,    // Sekreter (Randevular, ön ofis)
        PublicRelations,
        SiteManager,
        Architect,
        Engineer,
        Accountant,
        Purchasing
    }

    public class AgencyConsultant : BaseEntity
    {
        public int AgencyId { get; set; }
        public Agency Agency { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public bool IsActive { get; set; } = true; // Ofisten ayrılırsa false olur
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        public AgencyRole Role { get; set; } = AgencyRole.Consultant;
        
        // Finans / Bordro (Beyaz Yaka Maaş Sistemi)
        public decimal MonthlySalary { get; set; } = 0;
        public string? IBAN { get; set; }
        public string? IdentityNumber { get; set; } // TC Kimlik vb.
    }
}
