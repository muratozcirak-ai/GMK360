using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    // Profesyonel Tesis / Apartman Yönetim Şirketi (Ana Tenant)
    public class ManagementCompany : BaseEntity
    {
        public string CompanyName { get; set; }
        public string Subdomain { get; set; } // Örn: "vizyonyonetim" -> vizyonyonetim.domain.com
        
        public string TaxNumber { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        
        // Yönetim şirketinin yönettiği siteler
        public ICollection<HousingComplex> ManagedComplexes { get; set; } = new List<HousingComplex>();
    }
}
