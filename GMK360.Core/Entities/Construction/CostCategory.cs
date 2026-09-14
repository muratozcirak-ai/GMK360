using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction
{
    public class CostCategory : BaseEntity
    {
        public int? AgencyId { get; set; }
        public Agency Agency { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; } // e.g. "bi-truck", "bi-cup-hot"
        
        public PhaseItemType BaseType { get; set; } = PhaseItemType.Genel;

        public ICollection<TaskCost> TaskCosts { get; set; }
    }
}
