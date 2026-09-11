using System;

namespace GMK360.Core.Entities.Construction
{
    public class TaskMessage : BaseEntity
    {
        public int PhaseTaskId { get; set; }
        public PhaseTask PhaseTask { get; set; }

        public string SenderName { get; set; }
        public string SenderId { get; set; } // Orijinal User ID (string veya nullable)
        
        public string Message { get; set; }
        public string PhotoUrl { get; set; }
        
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
