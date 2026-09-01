using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities
{
    public class SystemFund : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public decimal Percentage { get; set; } = 0; // Örn: 10.5
        
        public decimal Balance { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
