using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class FiatTransaction : BaseEntity
    {
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossAmount { get; set; } // Brüt Hak Ediş

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } // Kesinti (Stopaj vb.)

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; } // Harcanabilir Net Bakiye

        [Required]
        public string Status { get; set; } = "Pending"; // Pending (Provizyon), Cleared (Kesinleşti), Withdrawn (Çekildi)

        [Required]
        public DateTime ClearsAt { get; set; } // Provizyonun bitip paranın netleşeceği tarih (Genelde 60 gün)
        
        public string? Description { get; set; }
    }
}
