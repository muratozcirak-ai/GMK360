using System;

namespace GMK360.Core.Entities
{
    public class PropertyFinancialRecord : BaseEntity
    {
        public int PropertyId { get; set; } // Kullanıcının "Dijital Evim" profili
        public Property Property { get; set; }

        public string ExpenseCategory { get; set; } // "Emlak Vergisi", "Aidat", "Tadilat"
        public string Description { get; set; } 
        public decimal TotalAmount { get; set; } 
        public decimal PaidAmount { get; set; } 
        public DateTime DueDate { get; set; } 
        public bool IsCompleted { get; set; } 
        
        // Dijital Arşiv (Fiş, Fatura, Dekont)
        public string DocumentUrl { get; set; }
    }
}
