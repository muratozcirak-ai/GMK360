using System;

namespace GMK360.Core.Entities.Construction
{
    public class DailyTimesheet : BaseEntity
    {
        public int AgencyId { get; set; }
        
        public int AgencyWorkerId { get; set; }
        public AgencyWorker AgencyWorker { get; set; }

        public DateTime WorkDate { get; set; }



        public string AttendanceStatus { get; set; } = "Tam Gün";
        
        public decimal EarnedWage { get; set; } 
        public decimal AdvancePayment { get; set; } 

        public string Notes { get; set; } 
        public string RecordedByUserId { get; set; }
    }
}
