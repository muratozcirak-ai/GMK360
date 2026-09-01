using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMK360.Core.Interfaces
{
    public class PointOfInterest
    {
        public string Name { get; set; }
        public string Type { get; set; } // Okul, Hastane, Metro vb.
        public double DistanceInMeters { get; set; }
        public int EstimatedWalkingTimeMinutes { get; set; }
    }

    public interface ILocationIntelligenceService
    {
        Task<List<PointOfInterest>> GetNearbyPointsOfInterestAsync(double latitude, double longitude, double radiusInMeters = 2000);
    }
}
