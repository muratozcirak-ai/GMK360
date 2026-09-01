namespace GMK360.Core.Entities
{
    public class Street : BaseEntity
    {
        public string Name { get; set; }
        public int NeighborhoodId { get; set; }
        public Neighborhood Neighborhood { get; set; }
        
        // Location Intelligence
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
