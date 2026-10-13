using System;

namespace GMK360.Core.Entities.Construction.SiteDiary
{
    // 2. AKSÝYONLAR VE ZAMAN OFSETÝ (T- / T+)
    public class TaskActionTemplate : BaseEntity
    {
        public int TaskTemplateId { get; set; }
        
        public string GorevTanimi { get; set; } // Örn: ""Beton santralinden randevu al""
        public int ZamanOfsetiGun { get; set; } // Örn: -2 (T-2 demek) veya +7
        public string HedefRol { get; set; } // Örn: ""Satýn Alma"", ""Þef"", ""Taþeron""

        public virtual TaskTemplate TaskTemplate { get; set; }
    }
}
