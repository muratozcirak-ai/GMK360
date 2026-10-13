using System;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class CustomerSelection : BaseEntity
    {
        public int CustomerUnitId { get; set; }
        public virtual CustomerUnit CustomerUnit { get; set; }

        public string SelectionCategory { get; set; } // Örn: "Zemin_Kaplama"
        
        // 1. Projenin bu daire için verdiği BEDAVA Standart Malzeme
        public int StandardMasterMaterialId { get; set; } 
        public virtual MasterMaterial StandardMasterMaterial { get; set; }
        
        // 2. Müşterinin fiilen seçtiği malzeme (Standart ile aynı da olabilir, Lüks de olabilir)
        public int SelectedMasterMaterialId { get; set; } 
        public virtual MasterMaterial SelectedMasterMaterial { get; set; }
        
        // 3. FİNANS: Müşteri lüks malzeme seçtiyse çıkacak fiyat farkı (Satış ofisine gidecek veri)
        public decimal UpgradePriceDifference { get; set; } // Örn: +25.000 TL
        
        public bool IsApprovedByCustomer { get; set; } // Müşteri imza attı mı?
    }
}
