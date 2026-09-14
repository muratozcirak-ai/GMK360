using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction
{
    public enum WarehouseType
    {
        Merkez = 1,
        Santiye = 2
    }

    public class Warehouse : BaseEntity
    {
        public int? AgencyId { get; set; }
        public Agency Agency { get; set; }

        public string Name { get; set; } // Merkez Depo, A Şantiyesi Deposu
        public WarehouseType Type { get; set; }

        public int? ConstructionProjectId { get; set; } // Şantiye deposu ise
        public ConstructionProject ConstructionProject { get; set; }
        
        public string Address { get; set; } // Depo Adresi
        public string Description { get; set; } // Açıklama
        
        public ICollection<InventoryItem> Items { get; set; }
    }
}
