using System;

namespace GMK360.Core.Entities
{
    public class UnitTemplateSpace : BaseEntity
    {
        public int UnitTemplateId { get; set; }
        public UnitTemplate UnitTemplate { get; set; }

        public string Name { get; set; } // Örn: Salon, Mutfak
        public string? Type { get; set; } // Örn: Yaşam Alanı, Islak Hacim

        public double? SquareMeters { get; set; }
        public string? Description { get; set; }
    }
}
