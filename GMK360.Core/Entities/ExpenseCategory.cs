using System;
using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities
{
    public class ExpenseCategory : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Örn: "Elektrik Faturası", "Aidat", "Çatı Bakımı"

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } // Örn: "electricity_bill", "dues", "maintenance"

        public bool IsTaxDeductible { get; set; } = false; // Vergiden düşülebilir mi?

        [MaxLength(50)]
        public string? Icon { get; set; } // FontAwesome veya SVG ikonu (örn: "fa-bolt")
    }
}
