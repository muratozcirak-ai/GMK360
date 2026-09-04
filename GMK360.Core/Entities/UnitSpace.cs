using System;

namespace GMK360.Core.Entities
{
    public class UnitSpace : BaseEntity
    {
        public int BuildingUnitId { get; set; }
        public BuildingUnit BuildingUnit { get; set; }

        public string Name { get; set; } 
        public string? Type { get; set; } 

        public double? SquareMeters { get; set; } 
        public string? PlanImagePath { get; set; } 

        public string? Description { get; set; } 
    }
}
