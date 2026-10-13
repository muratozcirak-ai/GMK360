using System;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class DxfUnitSpace : BaseEntity
    {
        public int DxfUnitTypeProfileId { get; set; }
        public DxfUnitTypeProfile UnitTypeProfile { get; set; }

        // Örn: "Salon", "Yatak Odası", "Ebeveyn Banyo"
        public string SpaceName { get; set; }
        public string SpaceGroup { get; set; } // Oda/Yaşam Alanı, Islak Hacim, vb.

        // Temel Geometrik DXF Okumaları
        public decimal FloorArea { get; set; } // Zemin m2
        public decimal CeilingArea { get; set; } // Tavan m2
        public decimal GrossWallArea { get; set; } // Brüt Duvar m2
        public decimal NetWallArea { get; set; } // Net Duvar m2 (Boşluklar Düşülmüş)
        
        public decimal SkirtingLength { get; set; } // Süpürgelik mt
        public decimal WindowSillLength { get; set; } // Denizlik mt
        
        // Demirbaş / Boşluk Sayımları (Sadece adet tutar, marka model tutmaz)
        public int InteriorDoorCount { get; set; }
        public int WindowCount { get; set; }
    }
}
