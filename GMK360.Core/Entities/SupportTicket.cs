using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class SupportTicket : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        
        // Status: Open, InProgress, Resolved, Cancelled
        public string Status { get; set; } = "Open";
        
        // Priority: Low, Normal, High, Urgent
        public string Priority { get; set; } = "Normal";
        
        // Category: Maintenance, MeetingRequest, Complaint, General
        public string Category { get; set; } = "General";

        // Yapan Kişi
        public string CreatorUserId { get; set; }
        public ApplicationUser CreatorUser { get; set; }

        // Sorumlu / Atanan Kişi
        public string AssignedToUserId { get; set; }
        public ApplicationUser AssignedToUser { get; set; }

        // Generic Context Fields
        public string ContextType { get; set; } // e.g. "Building", "Agency", "Property"
        public int? ContextId { get; set; }     // e.g. BuildingId
        public string ContextInfo { get; set; } // e.g. "Daire 43"
    }
}
