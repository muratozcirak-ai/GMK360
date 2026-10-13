using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities.Construction
{
    public class DailyTimesheet : BaseEntity
    {
        public int AgencyId { get; set; }
        
        public int AgencyWorkerId { get; set; }
        public AgencyWorker AgencyWorker { get; set; }

        // YENÝ EKLENEN ÞANTÝYE BAÐLANTISI
        public int? ConstructionProjectId { get; set; }
        public ConstructionProject? ConstructionProject { get; set; }
        
        public string? PhaseName { get; set; }

        public int? ProjectScheduleTaskId { get; set; } // O GUN HANGI GOREVDE CALISTI?
        public virtual GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask? ProjectScheduleTask { get; set; }

        public DateTime WorkDate { get; set; }

        // YENÝ EKLENEN PUANTAJ ZAMAN DÝLÝMLERÝ
        public bool IsMorningPresent { get; set; } = false; // Sabah yoklamasý
        public bool IsAfternoonPresent { get; set; } = false; // Öðle yoklamasý
        public decimal OvertimeHours { get; set; } = 0; // Mesai Saati

        public string AttendanceStatus { get; set; } = "Bekliyor"; // Tam Gün, Yarým Gün, Gelmedi, Mesaili
        
        public decimal PendingFieldExpense { get; set; } = 0; // Belge Bekleyen Saha Gideri (Elden Nakit)
        public string? FieldExpenseDescription { get; set; } // Örn: Yaðmur öncesi hýzlý döküm
        public decimal EarnedWage { get; set; }
 
        public decimal AdvancePayment { get; set; } 

        public string? Notes { get; set; } 
        public string? RecordedByUserId { get; set; }
    }
}

