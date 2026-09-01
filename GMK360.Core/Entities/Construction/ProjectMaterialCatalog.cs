using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction
{
    public class ProjectMaterialCatalog : BaseEntity
    {
        public int ConstructionProjectId { get; set; }
        public ConstructionProject ConstructionProject { get; set; }

        public string Category { get; set; } // Örn: "Mutfak Zemin Fayansı", "Banyo Dolabı"
        public string MaterialName { get; set; } // Örn: "Çanakkale Seramik - Siyah Mermer Desen"
        public string Description { get; set; }
        public string ImageUrl { get; set; } // Malzemenin görseli
        public decimal PriceDifference { get; set; } // Standart dışı, fark ödemeli bir seçenekse

        public virtual ICollection<UnitMaterialSelection> Selections { get; set; }
    }
}
