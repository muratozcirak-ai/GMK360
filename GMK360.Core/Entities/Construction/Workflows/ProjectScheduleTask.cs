using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class ProjectScheduleTask : BaseEntity
    {
        public int ProjectId { get; set; }
        public int? BuildingId { get; set; } // Hangi Yapı/Blok? (A Blok, Ortak Baza vb.)
        
        public string TaskName { get; set; } // Örn: "Radye Temel Betonarme Dökümü"
        public string? FloorOrZone { get; set; } // Hangi Kot/Kat
        public string? PartCategory { get; set; } // Kaba, Statik İnce, Daire İçi, Montaj
        public int PartIndex { get; set; } = 1; // 1: Kaba, 2: Statik İnce, 3: Daire İçi, 4: Montaj
        public int OrderIndex { get; set; }
        public bool IsCancelled { get; set; }
        
        // ŞEFİN INPUTLARI (GİRİŞ ALANLARI)
        public int PlannedDurationDays { get; set; } // Şefin girdiği iş süresi (gün)
        public int PlannedCrewSize { get; set; } // Şefin sahaya süreceği ekip (kişi)
        public decimal? Quantity { get; set; } // Şefin girdiği metraj/miktar
        public string? Unit { get; set; } // m³, ton, m²
        public string? MaterialTitle { get; set; } // İlgili malzeme cinsi
        public string? ProfessionName { get; set; } // Sorumlu Usta / Taşeron Branşı
        
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed
        
        public ICollection<ProjectScheduleSubTask> PrePreparations { get; set; } = new List<ProjectScheduleSubTask>();
    }

    public class ProjectScheduleSubTask : BaseEntity
    {
        public int ProjectScheduleTaskId { get; set; }
        public ProjectScheduleTask ProjectScheduleTask { get; set; }
        
        public string Title { get; set; } // Kontrol maddesi başlığı
        
        // 4 Kategori:
        // "PrePreparation"   -> 1. Ön Hazırlık (Malzeme & Tedarik & İzinler)
        // "Execution"        -> 2. İmalat (Sahada Uygulama)
        // "QualityAndCuring" -> 3. Kalite Kontrol & Kürlenme
        // "SafetyIsg"        -> 4. İSG Tedbirleri
        public string Category { get; set; } 
        
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? CompletedBy { get; set; }
        public string? Notes { get; set; }
        
        // BELGELEME / KANITLAR
        public bool RequiresPhoto { get; set; }
        public string? PhotoUrl { get; set; } // Yüklenen saha fotoğrafı
        public string? DocumentUrl { get; set; } // İrsaliye veya Test Raporu PDF
        public string? TestResultValue { get; set; } // Örn: "Slump: 18 cm", "28G Kırım: 34.2 MPa"
    }
}
