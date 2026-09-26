using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities.B2b
{
    public class B2bContact : BaseEntity
    {
        public int B2bCompanyId { get; set; }
        public B2bCompany B2bCompany { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } 

        [MaxLength(50)]
        public string? DepartmentOrRole { get; set; } // Muhasebe, Teknik Destek, Firma Sahibi

        [MaxLength(20)]
        public string? MobilePhone { get; set; }
        
        [MaxLength(20)]
        public string? LandlinePhone { get; set; }
        
        [MaxLength(10)]
        public string? ExtensionNumber { get; set; } // Dahili
        
        [MaxLength(100)]
        public string? Email { get; set; }
    }
}
