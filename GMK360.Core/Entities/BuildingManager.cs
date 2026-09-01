using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class BuildingManager : BaseEntity
    {
        public int BuildingId { get; set; }
        [ForeignKey("BuildingId")]
        public virtual Building Building { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public BuildingManagerRole Role { get; set; }

        // Yönetim devredildiğinde false yapılır, geçmiş veriyi tutmak için silinmez.
        public bool IsActive { get; set; } = true;
    }
}
