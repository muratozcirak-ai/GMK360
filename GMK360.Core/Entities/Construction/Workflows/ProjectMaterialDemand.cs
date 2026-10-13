using System;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class ProjectMaterialDemand : BaseEntity
    {
        public int ProjectId { get; set; }
        public int? ElevationId { get; set; }

        public int? ProjectScheduleTaskId { get; set; } // 5. MOTOR BAA LANTISI
        public virtual ProjectScheduleTask ProjectScheduleTask { get; set; }
        
        public int MasterMaterialId { get; set; } // Hangi malzeme lazým? (Katalogdan)
        public virtual MasterMaterial MasterMaterial { get; set; }
        
        public decimal TotalRequiredQuantity { get; set; } // Örn: 3250 (Adet/Kg)
        public bool IsOrdered { get; set; } // Satýnalma sipariþi geçti mi?
        public bool IsDeliveredToSite { get; set; } // Þantiyeye indi mi? (ProjectTask'taki IsMaterialReady'yi bu tetikleyecek!)
    }
}

