using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public enum ExpenseShareStatus
    {
        Pending = 1,
        Paid = 2
    }

    public class BuildingExpenseShare : BaseEntity
    {
        public int BuildingExpenseId { get; set; }
        [ForeignKey("BuildingExpenseId")]
        public virtual BuildingExpense BuildingExpense { get; set; }

        public int PropertyId { get; set; } // Hangi Daireye (Unit) kesildi?
        [ForeignKey("PropertyId")]
        public virtual Property Property { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShareAmount { get; set; } // Bu daireye düşen pay (Örn: 5.000 TL)

        public string ResponsibleUserId { get; set; } // Borçlu kim? (Tenant UserId veya Owner UserId)

        public ExpenseShareStatus Status { get; set; } = ExpenseShareStatus.Pending;

        public DateTime DueDate { get; set; }

        public DateTime? PaidDate { get; set; }

        public string? PaidByUserId { get; set; } // Ödemeyi fiilen kim yaptı? (Mahsuplaşma için kritik)
    }
}
