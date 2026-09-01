using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class UserFinancialProfile : BaseEntity
    {
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        public virtual Identity.ApplicationUser User { get; set; } = null!;

        public bool IsCorporate { get; set; } = false; // Fatura kesebiliyor mu?

        [MaxLength(100)]
        public string? TaxNumber { get; set; }
        
        [MaxLength(100)]
        public string? TaxOffice { get; set; }
        
        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [MaxLength(50)]
        public string? IBAN { get; set; }

        // Bireysel ise standart stopajdan farklı bir stopaj uygulanacaksa buraya girilir, null ise global ayar (GlobalFinanceSettings) geçerli olur.
        public decimal? CustomWithholdingTaxRate { get; set; } 
    }
}
