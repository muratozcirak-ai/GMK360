using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class BuildingUnit
    {
        public int Id { get; set; }
        
        public int BuildingId { get; set; }
        public virtual Building Building { get; set; }

        public string DoorNumber { get; set; }
        public int FloorLevel { get; set; } = 1;
        public string? FloorName { get; set; }
        
        [NotMapped]
        public string UnitNumber { get => DoorNumber; set => DoorNumber = value; }
        public double? GrossSquareMeters { get; set; }
        public double? NetSquareMeters { get; set; }
        public string FacadeDirection { get; set; }
        
        public string? RoomLayout { get; set; } // Eski UnitType - Örn: "3+1", "2+1"
        
        public int? UnitTypeId { get; set; }
        public DefinitionValue UnitType { get; set; }
        public bool IsAvailableForDailyRent { get; set; } = false;

        public string? FloorPlanUrl { get; set; } // Kat planı resmi
        
        // Resmi olmayan pratik takip için bilgiler
        public string? OwnerName { get; set; }
        public string? OwnerPhone { get; set; }
        public string? OwnerEmail { get; set; }
        
        public string? TenantName { get; set; }
        public string? TenantPhone { get; set; }
        public string? TenantEmail { get; set; }
        
        public string? OwnerUserId { get; set; }
        public Identity.ApplicationUser OwnerUser { get; set; }
        
        public string? TenantUserId { get; set; }
        public Identity.ApplicationUser TenantUser { get; set; }
        
        public bool IsEmpty { get; set; } // Daire boş mu?
        
        // Eğer bu daire sistemde bir Emlak İlanı/Mülkü (Property) ise eşleştirmek için (Opsiyonel)
        public int? PropertyId { get; set; }
        public virtual Property Property { get; set; }

                public int? UnitTemplateId { get; set; }
        public virtual GMK360.Core.Entities.UnitTemplate UnitTemplate { get; set; }

        public virtual System.Collections.Generic.ICollection<GMK360.Core.Entities.UnitSpace> Spaces { get; set; } = new System.Collections.Generic.List<GMK360.Core.Entities.UnitSpace>();
        public virtual System.Collections.Generic.ICollection<UnitDebt> Debts { get; set; } = new System.Collections.Generic.List<UnitDebt>();
    }
}
