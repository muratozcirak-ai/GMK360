using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction
{
    public enum PhaseItemType
    {
        Genel = 0,
        Malzeme = 1,
        Nakliye = 2,
        Kiralama = 3,
        Iscilik = 4,
        Taseron = 5
    }

    public class PhaseTask : BaseEntity
    {
        public int ProjectPhaseId { get; set; }
        public ProjectPhase ProjectPhase { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        
        public string Status { get; set; } = "Bekliyor"; // Bekliyor, Devam Ediyor, Tamamland
        public int OrderIndex { get; set; }

        
        public PhaseItemType ItemType { get; set; } = PhaseItemType.Genel;

        // --- KEŞİF / BÜTÇE PLANLAMA (Aday Proje Aşamasında Girilen Veriler) ---
        public decimal? EstimatedBudget { get; set; } // Planlanan Maliyet
        public decimal? EstimatedQuantity { get; set; } // Planlanan Miktar (Metraj)
        public string? Unit { get; set; } // Birim (m2, m3, Adet, kg vb.)


        public ICollection<TaskCost> TaskCosts { get; set; }
        public ICollection<TaskDocument> TaskDocuments { get; set; }
        public ICollection<TaskMessage> TaskMessages { get; set; }
        public ICollection<PhaseTaskTimesheet> Timesheets { get; set; }
    }
}
