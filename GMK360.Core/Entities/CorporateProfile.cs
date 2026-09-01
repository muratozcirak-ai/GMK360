using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class CorporateProfile : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; }

        [Required]
        [MaxLength(100)]
        public string TaxOffice { get; set; }

        [Required]
        [MaxLength(10)]
        public string TaxNumber { get; set; }

        // Bire-bir ilişki için
        public string ApplicationUserId { get; set; }
        
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
