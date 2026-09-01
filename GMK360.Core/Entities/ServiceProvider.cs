using System.Collections.Generic;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class ServiceProvider : BaseEntity
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string BusinessName { get; set; }
        public bool IsVerified { get; set; } = false; // Mavi Onay Rozeti

        public bool IsGlobal { get; set; } // Tüm Türkiye mi?
        
        public bool IsCorporate { get; set; } = false; // true: Kurumsal Firma, false: Yerel Usta

        // Bildirim ve Acil Çağrı (SOS) Özellikleri
        public bool ReceiveSmsNotifications { get; set; } = false; // Ücretli VIP SMS Onayı
        public bool IsEmergencyModeActive { get; set; } = false; // 7/24 Acil Usta / SOS Modu (Gece çalışma)
        public int ReliabilityScore { get; set; } = 100; // Gece çağrıya dönmeme durumlarında düşecek puan

        public int TotalRatings { get; set; } = 0;
        public double AverageRating { get; set; } = 0.0;

        public ICollection<ServiceProviderRating> Ratings { get; set; }
        public ICollection<ServiceProviderService> Services { get; set; }
        public ICollection<ServiceProviderArea> Areas { get; set; }
        public ICollection<ServiceAppointment> Appointments { get; set; }
        public ICollection<ServiceProviderPortfolio> Portfolios { get; set; }
        
        // Opsiyonel Resmi Evraklar (Vergi Levhası vb.)
        public virtual ICollection<ServiceProviderDocument> Documents { get; set; } = new List<ServiceProviderDocument>();
    }
}
