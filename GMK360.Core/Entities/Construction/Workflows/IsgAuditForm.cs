using System;
using System.Collections.Generic;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class IsgAuditForm : BaseEntity
    {
        public int ProjectId { get; set; }
        public virtual ConstructionProject Project { get; set; }
        
        public int? ElevationId { get; set; } // Hangi kat denetlendi? 
        public virtual ProjectElevation Elevation { get; set; }
        
        public DateTime AuditDate { get; set; }
        public string AuditorName { get; set; }
        public bool IsSiteSafe { get; set; }    
        
        public virtual ICollection<IsgChecklistItem> ChecklistItems { get; set; } = new List<IsgChecklistItem>();
    }
}
