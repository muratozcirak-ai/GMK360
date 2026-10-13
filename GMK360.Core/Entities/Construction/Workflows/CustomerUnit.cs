using System;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class CustomerUnit : BaseEntity
    {
        public int ProjectId { get; set; }
        public int ElevationId { get; set; } 
        
        public string UnitType { get; set; } // Örn: "Daire", "Ortak_Mahal", "Dükkan"
        public string UnitNumber { get; set; } // Örn: "Daire 12", "Kat 1 Koridor"
        public string? CustomerName { get; set; } // Ortak Mahal ise NULL kalır
        
        public decimal NetAreaSquareMeters { get; set; } // Metraj
    }
}
