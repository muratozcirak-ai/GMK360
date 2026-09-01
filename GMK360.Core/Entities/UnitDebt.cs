using System;

namespace GMK360.Core.Entities
{
    public class UnitDebt
    {
        public int Id { get; set; }
        
        public int BuildingUnitId { get; set; }
        public virtual BuildingUnit BuildingUnit { get; set; }

        public int BuildingExpenseId { get; set; }
        public virtual BuildingExpense BuildingExpense { get; set; }

        public decimal Amount { get; set; } // Daireye düşen pay (Örn: Toplam 10.000, Daireye düşen 1.000)
        
        public bool IsPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        
        // Ödemeyi kim yaptı? (Ahmet, Mehmet)
        public string PaidBy { get; set; }
    }
}
