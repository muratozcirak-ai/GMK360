using System;

namespace GMK360.Core.Entities
{
    public class SystemIssueTicket : BaseEntity
    {
        public string IssueType { get; set; } // e.g. "MapNotFound", "MissingStreet"
        public string Description { get; set; }
        public string ReportedByUserId { get; set; } // Nullable, if anonymous or not logged in yet
        public bool IsResolved { get; set; } = false;
        
        // Context Data
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public int? NeighborhoodId { get; set; }
        public string ContextData { get; set; } // Can store JSON or string like "StreetId: 15, Name: Papatya"
    }
}
