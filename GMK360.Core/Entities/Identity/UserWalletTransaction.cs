using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities.Identity;

public class UserWalletTransaction : BaseEntity
{
    [Required]
    public string ApplicationUserId { get; set; } = null!;
    
    [ForeignKey("ApplicationUserId")]
    public ApplicationUser? User { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; } // Cüzdana giren/çıkan NET tutar

    [Column(TypeName = "decimal(18,2)")]
    public decimal GrossAmount { get; set; } // Vergi öncesi Brüt tutar

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; } // Stopaj, Gelir Vergisi veya Fon kesintisi tutarı

    [MaxLength(100)]
    public string? TaxFundType { get; set; } // Örn: "Stopaj Fonu", "Gelir Vergisi Fonu"

    [Required]
    [MaxLength(50)]
    public string TransactionType { get; set; } = "TopUp"; // TopUp, ReferralBonus, ShowcasePurchase, ManualAdjustment, RentalIncome, Expense

    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int? RelatedPropertyId { get; set; }
    
    [ForeignKey("RelatedPropertyId")]
    public virtual Property? RelatedProperty { get; set; }
    
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
