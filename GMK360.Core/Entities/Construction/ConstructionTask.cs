using System;
using System.Collections.Generic;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities.Construction
{
    public class ConstructionTask : BaseEntity
    {
        public int ConstructionProjectId { get; set; }
        public ConstructionProject ConstructionProject { get; set; }

        public int? BuildingUnitId { get; set; } // Görev belirli bir daireye mi ait?
        public BuildingUnit BuildingUnit { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public string AssignedToUserId { get; set; } // Görev atanan Usta/Taşeron
        public ApplicationUser AssignedToUser { get; set; }

        public DateTime? DueDate { get; set; }
        
        // 0=Pending, 1=InProgress, 2=Completed
        public int Status { get; set; }

        public virtual ICollection<TaskProgressLog> ProgressLogs { get; set; }
    }
}
