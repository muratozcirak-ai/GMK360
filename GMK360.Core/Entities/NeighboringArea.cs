namespace GMK360.Core.Entities
{
    public class NeighboringArea : BaseEntity
    {
        public int BaseNeighborhoodId { get; set; }
        public Neighborhood BaseNeighborhood { get; set; }

        public int NeighborNeighborhoodId { get; set; }
        public Neighborhood NeighborNeighborhood { get; set; }

        public double DistanceInKilometers { get; set; }
    }
}
