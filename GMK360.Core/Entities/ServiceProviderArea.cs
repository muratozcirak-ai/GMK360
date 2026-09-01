namespace GMK360.Core.Entities
{
    public class ServiceProviderArea
    {
        public int ServiceProviderId { get; set; }
        public ServiceProvider ServiceProvider { get; set; }

        public int CityId { get; set; }
        public City City { get; set; }

        public int DistrictId { get; set; }
        public District District { get; set; }
    }
}
