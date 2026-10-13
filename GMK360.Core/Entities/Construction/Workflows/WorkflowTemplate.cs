using System;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class WorkflowTemplate : BaseEntity
    {
        public string PhaseCategory { get; set; } // "Temel", "TipKat", "Cati" (Bölge Ayrımı)
        public string WorkflowType { get; set; } // "Kaba", "InceSabit", "InceSecmeli" (3 Şeritli Otoban)
        public int StepOrder { get; set; } // İşin ilerleme sırası (1, 2, 3...)
        public string TaskName { get; set; }
        public string? WorkGroupCategory { get; set; } // Örn: Kaba_Kalıp
        public string? RequiredCondition { get; set; } // Karar Motoru Filtresi // Örn: "Grobeton Atılması"
        public bool IsGatekeeper { get; set; } // TRUE ise onay/vize gelmeden alt işe geçilmez
        public int WaitDays { get; set; } // İş bitince kaç gün beklenecek
        public string ExperienceNote { get; set; } // Şantiye Hafızası
    }
}
