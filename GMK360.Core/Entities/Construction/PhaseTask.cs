using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction
{
    public class PhaseTask : BaseEntity
    {
        public int ProjectPhaseId { get; set; }
        public ProjectPhase ProjectPhase { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        
        public string Status { get; set; } = "Bekliyor"; // Bekliyor, Devam Ediyor, Tamamlandı
        public int OrderIndex { get; set; }

        public ICollection<TaskCost> TaskCosts { get; set; }
        public ICollection<TaskDocument> TaskDocuments { get; set; }
        public ICollection<TaskMessage> TaskMessages { get; set; }
    }
}
