using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    public class Country : BaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; } // Örn: TR, US, GB vb.
        
        // Navigation Property
        public ICollection<City> Cities { get; set; }
    }
}
