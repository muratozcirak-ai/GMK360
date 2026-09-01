using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities
{
    public class LocalProfessional : BaseEntity
    {
        [Required]
        public string Name { get; set; } // Firma Adı veya Usta Adı (Örn: Çelik Kardeşler Nakliyat)

        [Required]
        public string ProfessionType { get; set; } // Örn: Nakliyeci, Boyacı, Nalbur, Tesisatçı, Elektrikçi

        public string? PhoneNumber { get; set; }
        
        public string? Address { get; set; }
        
        public string? LogoUrl { get; set; }
        
        public bool IsPremium { get; set; } // Ücret ödeyip üst sıraya çıkmak isteyen esnaflar için

        // Hangi mahalleye hizmet veriyor?
        public int NeighborhoodId { get; set; }
        public Neighborhood Neighborhood { get; set; }

        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        
        // Opsiyonel: Sisteme giriş yapması gerekiyorsa Identity User ile de bağlanabilir,
        // ancak kullanıcı "bunlar emlaktan ayrı olsun, panelleri vs farklı" dedi. 
        // Şimdilik Admin veya kendilerine özel panelden yönetilecek bir yapı.
        public string? IdentityUserId { get; set; } 
    }
}
