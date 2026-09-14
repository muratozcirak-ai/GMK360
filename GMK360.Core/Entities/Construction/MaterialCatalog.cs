using System;

namespace GMK360.Core.Entities.Construction
{
    // Şirket Malzeme Master Datası (Kataloğu)
    public class MaterialCatalog : BaseEntity
    {
        public int AgencyId { get; set; }
        public Agency Agency { get; set; }

        public string Name { get; set; } // Örn: "Kürek Sapı", "Çimento (50kg)"
        
        public InventoryItemType Type { get; set; } // 1 = Sarf, 2 = Demirbaş
        
        public string DefaultUnit { get; set; } // Adet, Kg, Ton, m2 vb.
        
        public string DefaultBrand { get; set; } // İsteğe bağlı varsayılan marka
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
