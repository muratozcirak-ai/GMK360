using System.Collections.Generic;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class Agency : BaseEntity
    {
        public string CompanyName { get; set; }
        public string TaxNumber { get; set; }
        public string AuthCertificateNo { get; set; } // Taşınmaz Ticareti Yetki Belge No
        public bool IsMinistryVerified { get; set; } // Bakanlık Onay (TTBS)
        public string Address { get; set; }
        public string LogoUrl { get; set; }
        public string WhatsAppNumber { get; set; }
        public int PackageLimit { get; set; } // Aylık ilan kotası
        
        // Kurumsal Mağaza (White-label) Özellikleri
        public string Subdomain { get; set; } // Örn: remax-yildiz
        public string CustomDomain { get; set; } // Örn: www.kendiofisdomaini.com
        public string ThemePrimaryColor { get; set; } // Örn: #FF0000
        public string ThemeSecondaryColor { get; set; } // Örn: #000000
        public string FontFamily { get; set; } // Örn: 'Inter', 'Roboto'
        public string BannerUrl { get; set; } // Mağaza kapak fotoğrafı
        
        public string FooterLogoUrl { get; set; }
        public string FaviconUrl { get; set; }
        public string ThemeAccentColor { get; set; } // Örn: #FFC107
        
        // CRM & Sözleşme Takibi
        public System.DateTime ContractStartDate { get; set; }
        public System.DateTime ContractEndDate { get; set; }
        public bool IsActive { get; set; } // Sözleşmesi biten veya iptal edilen emlakçılar için false
        
        public ICollection<AgencyConsultant> AgencyConsultants { get; set; }
        public ICollection<AgencySubscription> Subscriptions { get; set; }
    }
}
