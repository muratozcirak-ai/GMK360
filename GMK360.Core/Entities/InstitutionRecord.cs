using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    public class InstitutionRecord
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Address { get; set; }
        public string GooglePlaceId { get; set; }
        public string GoogleMapsUrl { get; set; }

        public ICollection<InstitutionContact> Contacts { get; set; }
    }
}
