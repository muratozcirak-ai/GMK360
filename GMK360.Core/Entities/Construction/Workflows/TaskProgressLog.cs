using System;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class TaskProgressLog : BaseEntity
    {
        public int ProjectTaskId { get; set; } // Hangi görev?
        public int SubcontractorId { get; set; } 
        
        public DateTime ReportDate { get; set; }
        public decimal CompletedQuantity { get; set; } // Bugün kaç m2 ördü? (Örn: 200)
        
        public bool IsApprovedByChief { get; set; } // Şef onayladı mı?
        public decimal CalculatedEarnedValue { get; set; } // Hakediş Tutarı (200 * 150 TL = 30.000 TL)
    }
}
