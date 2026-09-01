using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class PropertyUser : BaseEntity
    {
        [Required]
        public int PropertyId { get; set; }
        [ForeignKey("PropertyId")]
        public virtual Property Property { get; set; }

        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        /// <summary>
        /// 1 = Ev Sahibi (Owner)
        /// 2 = Kiracı (Tenant)
        /// 3 = Aile Ferdi / Oturan (Occupant)
        /// </summary>
        [Required]
        public int RoleType { get; set; } 

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Null ise aktif olarak oturuyor/sahibi demektir. Çıkış yaptığında burası doldurulur.
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
