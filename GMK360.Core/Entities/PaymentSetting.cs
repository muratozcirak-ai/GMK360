using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class PaymentSetting : BaseEntity
    {
        [Required]
        public string UserId { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        [MaxLength(255)]
        public string? IyzicoApiKey { get; set; }

        [MaxLength(255)]
        public string? IyzicoSecretKey { get; set; }

        public bool PassCommissionToTenant { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }
}
