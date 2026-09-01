using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class SystemAuditLog
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string ActionType { get; set; } // Örn: MERGE_BUILDING
        
        [Required]
        public string Description { get; set; } // Örn: "Yaşer Bey Apt (ID:15), Yaşar Bey Apt (ID:20) ile birleştirildi ve silindi."
        
        [Required]
        [MaxLength(100)]
        public string PerformedByUserId { get; set; } // Admin'in UserId'si
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
