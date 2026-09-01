using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction
{
    public class ConstructionProject : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        
        public int AgencyId { get; set; }
        public Agency Agency { get; set; }

        public string? CoverImageUrl { get; set; }
        public string Address { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // 0=Upcoming, 1=Ongoing, 2=Completed
        public int Status { get; set; }

        public DateTime SelectionDeadline { get; set; }

        public virtual ICollection<ProjectPhase> Phases { get; set; }
        public virtual ICollection<ProjectMaterialCatalog> MaterialCatalogs { get; set; }
        public virtual ICollection<ConstructionTask> Tasks { get; set; }
        
        // Projeye ait bloklar / binalar
        public virtual ICollection<Building> Blocks { get; set; }
    }
}
