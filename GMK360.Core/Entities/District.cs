namespace GMK360.Core.Entities
{
    public class District : BaseEntity
    {
        public string Name { get; set; }
        public int CityId { get; set; }
        public City City { get; set; }
        public string? RegionName { get; set; } // Örn: "Avrupa Yakası", "Anadolu Yakası"
    }
}
