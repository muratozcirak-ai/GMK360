using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities.Construction
{
    public class PhaseMessage : BaseEntity
    {
        public int ProjectPhaseId { get; set; }
        public ProjectPhase ProjectPhase { get; set; }

        public string SenderUserId { get; set; }
        
        public string Content { get; set; }
    }
}
