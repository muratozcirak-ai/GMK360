using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class ProjectScheduleTask : BaseEntity
    {
        public int ProjectId { get; set; }
        
        public string TaskName { get; set; } // Örn: "4. Kat Kolon Kalıbı ve Demir Döşeme"
        public string FloorOrZone { get; set; } // Hangi Kot/Kat
        public int OrderIndex { get; set; }
        public bool IsCancelled { get; set; }
        
        public int PlannedDurationDays { get; set; } // Mühendisin atayacağı süre (0 ise henüz planlanmamış)
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        
        public string Status { get; set; } // Pending, InProgress, Completed
        
        public ICollection<ProjectScheduleSubTask> PrePreparations { get; set; } = new List<ProjectScheduleSubTask>();
    }

    public class ProjectScheduleSubTask : BaseEntity
    {
        public int ProjectScheduleTaskId { get; set; }
        public ProjectScheduleTask ProjectScheduleTask { get; set; }
        
        public string Title { get; set; } // Örn: "Kule Vinç ayarlaması yap", "Depodan demir kontrolü"
        public string Category { get; set; } // Logistics, Inventory, Safety (İSG), Material
        public bool IsCompleted { get; set; }
        public bool RequiresPhoto { get; set; } // İSG ve kalite kontrol için resim zorunluluğu
    }
}
