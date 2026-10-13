using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class IsgChecklistItem : BaseEntity
    {
        public int IsgAuditFormId { get; set; }
        public virtual IsgAuditForm IsgAuditForm { get; set; }
        
        public string Question { get; set; }    // Örn: "Şaft boşluklarında korkuluk var mı?"
        public bool IsPassed { get; set; }      // Geçti / Kaldı
        public bool IsCritical { get; set; }    // TRUE ise ve IsPassed FALSE ise, ProjectElevation'ı kilitler!
        public string? ActionRequired { get; set; } 
    }
}
