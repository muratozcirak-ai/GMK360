using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities.Construction
{
    public class DailyTimesheet : BaseEntity
    {
        public int AgencyId { get; set; }
        
        public int AgencyWorkerId { get; set; }
        public AgencyWorker AgencyWorker { get; set; }

        // YENİ EKLENEN ŞANTİYE BAĞLANTISI
        public int? ConstructionProjectId { get; set; }
        public ConstructionProject? ConstructionProject { get; set; }
        
        public string? PhaseName { get; set; } // Hangi faza/işe atandı? (Örn: "3. Faz - Kalıp")

        public DateTime WorkDate { get; set; }

        // YENİ EKLENEN PUANTAJ ZAMAN DİLİMLERİ
        public bool IsMorningPresent { get; set; } = false; // Sabah yoklaması
        public bool IsAfternoonPresent { get; set; } = false; // Öğle yoklaması
        public decimal OvertimeHours { get; set; } = 0; // Mesai Saati

        public string AttendanceStatus { get; set; } = "Bekliyor"; // Tam Gün, Yarım Gün, Gelmedi, Mesaili
        
        public decimal PendingFieldExpense { get; set; } = 0; // Belge Bekleyen Saha Gideri (Elden Nakit)
        public string? FieldExpenseDescription { get; set; } // Örn: Yağmur öncesi hızlı döküm
        public decimal EarnedWage { get; set; }
 
        public decimal AdvancePayment { get; set; } 

        public string? Notes { get; set; } 
        public string? RecordedByUserId { get; set; }
    }
}
