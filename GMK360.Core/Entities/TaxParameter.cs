using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class TaxParameter : BaseEntity
    {
        public int TaxYear { get; set; } // Hangi yılın vergi parametreleri? (Örn: 2026)

        [Column(TypeName = "decimal(18,2)")]
        public decimal ResidentialExemptionAmount { get; set; } // Konut Kira Geliri İstisna Tutarı

        [Column(TypeName = "decimal(18,2)")]
        public decimal CommercialExemptionAmount { get; set; } // İşyeri Beyan Sınırı

        // Vergi dilimlerinin (Tax Brackets) JSON tablosu. Örneğin: 
        // [{"limit": 110000, "rate": 0.15}, {"limit": 230000, "rate": 0.20}, ...]
        public string? TaxBracketsJson { get; set; } 
    }
}
