namespace GMK360.Core.Entities
{
    public class Neighborhood : BaseEntity
    {
        public string Name { get; set; }
        public string? ZipCode { get; set; } // Posta Kodu
        public int DistrictId { get; set; }
        public District District { get; set; }
        
        // Location Intelligence
        public string? Latitude { get; set; }
                public string? Longitude { get; set; }

        // AI Insight Caching
        public string? AiAnalysisReport { get; set; }
        public DateTime? AiAnalysisUpdatedAt { get; set; }
    }
}

