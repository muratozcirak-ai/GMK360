using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction.SiteDiary
{
    // 1. ANA ÝÞ PAKETÝ (Master)
    public class TaskTemplate : BaseEntity
    {
        public string PaketAdi { get; set; } // Örn: ""Kat Betonu Dökülmesi""
        public bool AktifMi { get; set; } = true;
        
        // Ýliþkiler
        public virtual ICollection<TaskActionTemplate> Aksiyonlar { get; set; }
        public virtual ICollection<TaskChecklistTemplate> Checklistler { get; set; }
    }
}
