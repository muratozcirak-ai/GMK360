using System;

namespace GMK360.Core.Entities.Construction
{
    public class PhaseTaskTimesheet : BaseEntity
    {
        public int PhaseTaskId { get; set; }
        public PhaseTask PhaseTask { get; set; }

        public string WorkerName { get; set; }
        public DateTime WorkDate { get; set; }
        
        public double HoursWorked { get; set; }
        public decimal DailyWage { get; set; } // Gunluk veya saatlik ucret (toplam= HoursWorked * DailyWage eger saatlikse, veya direk gunluk yevmiye)
        
        public decimal FoodExpense { get; set; }
        public decimal TravelExpense { get; set; }
        
        public string Notes { get; set; }
    }
}
