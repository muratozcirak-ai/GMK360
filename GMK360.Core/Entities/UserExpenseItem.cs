using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class UserExpenseItem : BaseEntity
    {
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public int? BuildingId { get; set; } // EYer bir binaya baYlysa
        public virtual Building Building { get; set; }

        [Required]
        public int ExpenseCategoryId { get; set; } // -rn: 2 = letiYim
        [ForeignKey("ExpenseCategoryId")]
        public virtual ExpenseCategory ExpenseCategory { get; set; }

        [Required]
        [MaxLength(150)]
        public string CustomLabel { get; set; } // -rn: "Kzmn Hatt", "Netflix Aile"

        // UYKUDAK KURUMLAR MMARS (Gelecek API HazrlY)
        public int? InstitutionId { get; set; } // -rn: GDAz, Turkcell (Gelecekte eklenecek tablo)
        
        [MaxLength(100)]
        public string SubscriberNumber { get; set; } // Abone No / Tesisat No
    }
}
