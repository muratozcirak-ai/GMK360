using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class SpaceMeasurement : BaseEntity
    {
        public int UnitSpaceId { get; set; }
        public virtual UnitSpace UnitSpace { get; set; }

        public string Category { get; set; } // Zemin, Duvar, Tavan, Tesisat vb.
        public string Description { get; set; } // Ince Siva, Zemin Sapi, Fayans Kaplama vb.
        
        public double Quantity { get; set; }
        public string Unit { get; set; } // m2, mt, adet
    }
}
