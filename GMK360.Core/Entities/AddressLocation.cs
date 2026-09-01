using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    public class AddressLocation : BaseEntity
    {
        public string City { get; set; }
        public string District { get; set; }
        public string Neighborhood { get; set; }
        public string ComplexName { get; set; } // Site adı vb.
        
        // Relational properties
        public ICollection<Property> Properties { get; set; }
    }
}
