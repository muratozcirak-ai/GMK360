using System;

namespace GMK360.Core.Entities.Construction
{
    public class PhaseWorkerDemand : BaseEntity
    {
        public int ProjectPhaseId { get; set; }
        public ProjectPhase ProjectPhase { get; set; }

        public string RequiredProfession { get; set; }
        public int RequiredQuantity { get; set; } 
        
        public DateTime TargetDate { get; set; } 
        public string Description { get; set; }

        public string Status { get; set; } = "Açık"; // Açık, Karşılandı, İptal
        public string CreatedByUserId { get; set; }
    }
}
