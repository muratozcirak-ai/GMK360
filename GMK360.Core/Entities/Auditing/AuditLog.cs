using System;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Auditing
{
    public class AuditLog : BaseEntity
    {
        public string UserId { get; set; }
        public string ActionType { get; set; } 
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string OldValues { get; set; } 
        public string NewValues { get; set; } 
        public string AffectedColumns { get; set; } 
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
