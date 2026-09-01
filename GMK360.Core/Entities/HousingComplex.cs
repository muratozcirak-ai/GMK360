using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    // Yönetim Şirketine bağlı "Site / Proje" kavramı
    public class HousingComplex : BaseEntity
    {
        // B2B: Yönetim Şirketi FK (Bireysel kayıtlı siteler için null olabilir)
        public int? ManagementCompanyId { get; set; }
        public ManagementCompany ManagementCompany { get; set; }
        
        public string Name { get; set; } // Örn: "Mavi Akdeniz Evleri"
        
        // 5'li Konum Hiyerarşisi (Sitenin ana adresi)
        public int CityId { get; set; }
        public virtual City City { get; set; }

        public int DistrictId { get; set; }
        public virtual District District { get; set; }

        public int NeighborhoodId { get; set; }
        public virtual Neighborhood Neighborhood { get; set; }

        public int? StreetId { get; set; }
        public virtual Street Street { get; set; }

        public string Address { get; set; }
        
        public int TotalBlocks { get; set; } // İçindeki blok/bina sayısı
        
        // Sitenin Özellikleri (Havuz, Güvenlik vb.)
        public ICollection<ComplexFeature> Features { get; set; } = new List<ComplexFeature>();

        // Siteye ait tüm binalar/bloklar
        public ICollection<Building> Buildings { get; set; } = new List<Building>();
    }
}
