using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities.B2b
{
    
    public enum B2bInvitationStatus
    {
        Shadow = 0,      // 🌑 Gölge (Davet Edilmedi)
        Invited = 1,     // 📨 Davet Gönderildi
        Active = 2,      // 🟢 Aktif (Sisteme Girdi)
        Pro = 3          // 👑 Pro Kullanıcı
    }

    public enum LegalEntityType
    {
        Individual = 1, // Şahıs
        Corporate = 2   // Tüzel Kişi (Şirket)
    }

    public class B2bCompany : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public LegalEntityType LegalStatus { get; set; } = LegalEntityType.Individual;

        // Faaliyet Rolleri (Çoklu Seçilebilir)
        public bool IsSupplier { get; set; } = false;      // Malzeme Tedarikçisi
        public bool IsSubcontractor { get; set; } = false; // Taşeron / Hizmet
        public bool IsEngineering { get; set; } = false;   // Mühendislik / Denetim / Profesyonel Hizmet
        
        // Kurumsal Yapı (Şubeler / Bayi Ağı var mı?)
        public bool IsEnterprise { get; set; } = false;

        [MaxLength(50)]
        public string? TaxOffice { get; set; }
        
        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        
        public string? AddedByUserId { get; set; }
        public Identity.ApplicationUser? AddedByUser { get; set; }
        
        public int? AddedByAgencyId { get; set; }
        public Agency? AddedByAgency { get; set; }
        
        public bool IsVerified { get; set; } = false;
        public B2bInvitationStatus InvitationStatus { get; set; } = B2bInvitationStatus.Shadow;
        
        [Range(0, 5)]
        public double Rating { get; set; } = 0;

        public ICollection<B2bCompanyCategory> CompanyCategories { get; set; } = new List<B2bCompanyCategory>();
        public ICollection<B2bBranch> Branches { get; set; } = new List<B2bBranch>();
        public ICollection<B2bContact> Contacts { get; set; } = new List<B2bContact>();
    }
}
