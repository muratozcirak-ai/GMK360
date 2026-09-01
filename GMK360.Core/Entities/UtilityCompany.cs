using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    public class UtilityCompany : BaseEntity
    {
        public string Name { get; set; } = null!; // Örn: İSKİ, İGDAŞ, BEDAŞ
        public string Type { get; set; } = null!; // Örn: Su, Elektrik, Doğalgaz, İnternet
        public bool IsActive { get; set; } = true;
        public string? City { get; set; } // Sadece belirli bir şehre hizmet veriyorsa (opsiyonel)

        public virtual ICollection<PropertyLiability> Liabilities { get; set; } = new List<PropertyLiability>();
    }
}
