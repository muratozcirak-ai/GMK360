using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class B2bSupplier : BaseEntity
    {
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public string CompanyName { get; set; }
        public string Phone { get; set; }
        
        public int CityId { get; set; }
        public City City { get; set; }
        
        public int DistrictId { get; set; }
        public District District { get; set; }
        
        // Örn: Nalbur, Elektrikçi, Hırdavatçı vs.
        public string Category { get; set; } 
        
        public bool IsActive { get; set; } = true;
    }
}
