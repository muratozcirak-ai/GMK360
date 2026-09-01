using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    // Daha önce oluşturulmuş ServiceCategory olabilir, kontrol etmek adına yeni ekliyoruz.
    public class ServiceCategory : BaseEntity
    {
        public string Name { get; set; } // Örn: Boya-Badana, Su Tesisatı, Temizlik
        public string Slug { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string IconClass { get; set; } // ph-paint-roller vs
        public bool IsActive { get; set; } = true;
        
        public ICollection<ServiceProviderService> ProviderServices { get; set; }
    }
}
