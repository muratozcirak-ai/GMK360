using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class MaterialOption : BaseEntity
    {
        public int SpaceFixtureId { get; set; }
        public virtual SpaceFixture SpaceFixture { get; set; }

        public string OptionName { get; set; } // Masif Ahsap, Koyu Gri vs
        public decimal PriceDifference { get; set; } = 0; // +15000 TL gibi
        
        public string? ImageUrl { get; set; }
        public bool IsStandard { get; set; } = false;
    }
}
