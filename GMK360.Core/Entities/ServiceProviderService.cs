namespace GMK360.Core.Entities
{
    public class ServiceProviderService
    {
        public int ServiceProviderId { get; set; }
        public ServiceProvider ServiceProvider { get; set; }

        public int ServiceCategoryId { get; set; }
        public ServiceCategory ServiceCategory { get; set; }

        public decimal StartingPrice { get; set; } = 0; // Hizmet taban fiyatı
    }
}
