using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public enum AccountType
    {
        Individual,
        Corporate
    }

    public class FinancialAccount : BaseEntity
    {
        // Bireysel Kullanıcı için
        public string? AppUserId { get; set; }
        public ApplicationUser AppUser { get; set; }

        // Kurumsal / Site Yönetimi vb. için
        public int? TenantId { get; set; } // Eğer multi-tenant yapı varsa, yoksa ManagementCompanyId / HousingComplexId
        public int? ManagementCompanyId { get; set; }
        public ManagementCompany ManagementCompany { get; set; }

        public AccountType AccountType { get; set; }

        // İyzico/Pazaryeri alt üye işyeri verileri (SubMerchantAccount yerine kullanılıyor)
        public string IBAN { get; set; }
        public string? SubMerchantKey { get; set; } // Ödeme geçidinden dönecek anahtar
        public string? CompanyName { get; set; }
        public string? TaxNumber { get; set; }
        public string? TaxOffice { get; set; }
        
        public bool IsActive { get; set; } = false; // IBAN/Bilgiler doğrulanınca aktifleşir
    }
}
