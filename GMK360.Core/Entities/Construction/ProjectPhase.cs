using System;

namespace GMK360.Core.Entities.Construction
{
    public class ProjectPhase : BaseEntity
    {
        public int ConstructionProjectId { get; set; }
        public ConstructionProject ConstructionProject { get; set; }

        public string Name { get; set; } // Örn: Temel Atma, Kaba İnşaat, İnce İşçilik
        public string Description { get; set; }
        public int OrderIndex { get; set; } // Aşama sırası
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        
        // 0=Pending, 1=InProgress, 2=Completed
        public int Status { get; set; }
    }
}
