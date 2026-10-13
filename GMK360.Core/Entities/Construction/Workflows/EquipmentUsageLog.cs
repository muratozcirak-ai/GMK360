using System;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class EquipmentUsageLog : BaseEntity
    {
        public int ConstructionProjectId { get; set; }
        public int CompanyEquipmentId { get; set; }
        
        public DateTime StartDate { get; set; } // Kule vinç kurulma veya araç çağırma tarihi
        public DateTime? EndDate { get; set; }  // Bitiş tarihi
        
        public TimeSpan? StartTime { get; set; } // Saatlik kiralamalar için başlama saati
        public TimeSpan? EndTime { get; set; }   // Saatlik kiralamalar için bitiş saati
        
        public string UsageType { get; set; } // "Saatlik", "Günlük", "Aylık", "Proje Boyunca"
        
        public int Duration { get; set; } // Süre (Kaç saat, kaç gün vb.)
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; } // Bedeli hesaplanıp buraya yazılacak
        
        // Navigation Properties
        public ConstructionProject ConstructionProject { get; set; }
        public CompanyEquipment CompanyEquipment { get; set; }
    }
}
