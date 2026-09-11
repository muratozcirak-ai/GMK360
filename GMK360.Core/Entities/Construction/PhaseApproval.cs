using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities.Construction
{
    public class PhaseApproval
    {
        [Key]
        public int Id { get; set; }

        public int ProjectPhaseId { get; set; }
        
        [ForeignKey("ProjectPhaseId")]
        public virtual ProjectPhase ProjectPhase { get; set; }

        public string ApprovedByUserId { get; set; } // ONAYLAYAN PATRON (User ID)

        public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
    }
}
