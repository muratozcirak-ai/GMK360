using System;

namespace GMK360.Core.Entities
{
    public class PropertyPriceHistory : BaseEntity
    {
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string Currency { get; set; } // TRY, USD, EUR, GBP
        public DateTime ChangedAt { get; set; }
    }
}
