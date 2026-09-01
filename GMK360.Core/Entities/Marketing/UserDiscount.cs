using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities.Marketing
{
    public class UserDiscount
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        
        public int DiscountPolicyId { get; set; }
        public virtual DiscountPolicy DiscountPolicy { get; set; }
        
        public bool IsUsed { get; set; } = false; // Tek kullanımlıksa True yapılır
        
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string AssignedByStaffId { get; set; } // Bu özel iskontoyu veren Satış Personeli ID'si
    }
}
