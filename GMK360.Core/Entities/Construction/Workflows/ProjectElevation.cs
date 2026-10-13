using System;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class ProjectElevation : BaseEntity
    {
        public int ProjectId { get; set; }

        public int? ParentElevationId { get; set; }
        public ProjectElevation? ParentElevation { get; set; }
        
        public string? BlockName { get; set; } // "Ortak Baza", "A Kulesi"
        
        // İSG Kilit Mekanizması
        public bool IsLockedByIsg { get; set; }
        public string? IsgLockReason { get; set; }
 // Hangi proje?
        public ConstructionProject Project { get; set; }

        public string Name { get; set; } // Örn: "-1. Bodrum", "Zemin Kat", "5. Kat"
        public int ElevationLevel { get; set; } // Mantıksal Sıralama: -2, -1, 0, 1, 2, 3...
        public bool IsBasement { get; set; } // Hafriyat/İksa kuralları için bodrum ayrımı
    }
}
