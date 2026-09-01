using System.Collections.Generic;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public enum PackagePeriod
    {
        Monthly,
        Yearly,
        Lifetime, // Tek Seferlik (Ömür Boyu) - Ustalar için vs.
        Free      // Ücretsiz
    }

    public class SubscriptionPackage : BaseEntity
    {
        public string Name { get; set; } = null!; // Örn: Standart Paket, Premium Paket, Yerel Usta Paketi
        public decimal Price { get; set; } // Aylık veya Yıllık Fiyat
        
        public PackagePeriod Period { get; set; }
        public UserType? TargetUserType { get; set; } // Hangi kullanıcı tipine hitap ediyor? (Boş ise genel)
        
        // Sınırlamalar
        public int? MaxListingCount { get; set; } // Satıcı/Kiralayan ilan limiti
        public int? MaxPropertiesCount { get; set; } // Mülk sahibi için mülk sınırı (Örn: 2)
        public int? MaxNeighborhoodsCount { get; set; } // Usta için mahalle sınırı
        public int? MaxFeaturedListingCount { get; set; } // Vitrin (Doping) hakkı
        
        // Bireysel Kullanıcı (Dijital Evim) SaaS Limitleri
        public int? MaxRentTrackingCount { get; set; } // Kira Geliri Takip Limiti (Örn: Başlangıç=3, Pro=Sınırsız)
        public bool HasSmsNotifications { get; set; } // E-Posta haricinde SMS bildirimi hakkı var mı?
        
        public bool IsActive { get; set; } = true;

        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}
