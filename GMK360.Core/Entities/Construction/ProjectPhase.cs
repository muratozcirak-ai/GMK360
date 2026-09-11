using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction
{
    public class ProjectPhase : BaseEntity
    {
        public int ConstructionProjectId { get; set; }
        public ConstructionProject ConstructionProject { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int OrderIndex { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        
        public int Status { get; set; }

        public int RequiredApprovals { get; set; } = 1;
        public virtual ICollection<PhaseApproval> PhaseApprovals { get; set; } = new List<PhaseApproval>();
        public virtual ICollection<PhaseMessage> PhaseMessages { get; set; } = new List<PhaseMessage>();
        public virtual ICollection<PhaseTask> PhaseTasks { get; set; } = new List<PhaseTask>();
    }
}
