using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class GlobalObligationRule : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string RuleName { get; set; } // Örn: "2026 Mesken Kira Stopaj Oranı", "Emlak Vergisi 1. Taksit"

        [Required]
        [Column(TypeName = "decimal(5,4)")]
        public decimal Rate { get; set; } // Örn: 0.20 (%20)

        public bool IsActive { get; set; } = true;

        public DateTime StartDate { get; set; } // Kuralın geçerlilik başlangıcı
        public DateTime? EndDate { get; set; } // Kuralın bitişi (Devlet oranı değiştirdiğinde buraya tarih atılır ve yeni kural eklenir)

        // Hangi gider kategorisine uygulanacak?
        public int? TargetExpenseCategoryId { get; set; }
        [ForeignKey("TargetExpenseCategoryId")]
        public virtual ExpenseCategory? TargetExpenseCategory { get; set; }
    }
}
