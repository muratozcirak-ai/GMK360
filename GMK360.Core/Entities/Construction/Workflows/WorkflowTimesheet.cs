using System;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class WorkflowTimesheet : BaseEntity
    {
        public int CrewId { get; set; } // Hangi ekip/işçi?
        public DateTime Date { get; set; }
        
        // Kayıp Zaman Yönetimi
        public string WorkStatus { get; set; } // "Çalıştı", "Beklemede (Stand-by)"
        public string StandbyReason { get; set; } // Yatış Sebebi: "Malzeme Gecikti", "Vinç Bozuldu", "Hava Muhalefeti"
        public decimal DailyWage { get; set; } // Yevmiye maliyeti (Beklese bile şirkete yazan zarar)
    }
}
