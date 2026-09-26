using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities.Construction
{
    public class AgencyPhonebookContact : BaseEntity
    {
        public int AgencyPhonebookId { get; set; }
        public AgencyPhonebook AgencyPhonebook { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; } 

        [MaxLength(50)]
        public string? DepartmentOrRole { get; set; } 

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        
        [MaxLength(10)]
        public string? ExtensionNumber { get; set; } 
        
        [MaxLength(100)]
        public string? Email { get; set; }
    }
}
