using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities.Marketing
{
    public class DiscountPolicy
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Örn: "2. Aboneliğe %25 İndirim" VEYA "Yaz Kampanyası"
        
        public string Description { get; set; }
        
        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; } // % Kaç indirim?
        
        public decimal? FixedDiscountAmount { get; set; } // Sabit tutar indirimi de olabilir
        
        // Otomatik Çapraz Satış Kuralı
        public int MinimumActiveSubscriptionsRequired { get; set; } = 0; // Eğer 1 ise, kullanıcının en az 1 aktif aboneliği varsa bu indirim devreye girer.
        
        public bool IsActive { get; set; } = true;
        
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        
        // Bu kuralın uygulandığı özel kullanıcılar (Opsiyonel)
        public virtual ICollection<UserDiscount> UserDiscounts { get; set; } = new List<UserDiscount>();
    }
}
