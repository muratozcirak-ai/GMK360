namespace GMK360.Core.DTOs
{
    public class PropertyValuationRequestDto
    {
        public string City { get; set; }
        public string District { get; set; }
        public string Neighborhood { get; set; }
        
        public int NetSquareMeters { get; set; }
        public int RoomCount { get; set; }
        public int BuildingAge { get; set; }
        public int FloorLevel { get; set; }
        public string Facade { get; set; } // Örn: Güney, Kuzey
        
        public bool HasElevator { get; set; }
        public bool HasParking { get; set; }
        public bool IsFurnished { get; set; }
        
        public string AdditionalFeatures { get; set; } // Örn: Yeni boyandı, lüks ankastre vb.
    }
}
