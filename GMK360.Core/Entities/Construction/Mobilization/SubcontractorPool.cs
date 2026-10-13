using GMK360.Core.Entities.Auditing;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction.Mobilization
{
    // Sistemdeki veya firmadaki taþeron/usta havuzu
    public class SubcontractorPool
    {
        public int Id { get; set; }
        public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;
        public string AdSoyadUnvan { get; set; } // Örn: Ali Usta Kalýp Ekibi
        public string Brans { get; set; } // Örn: Kaba Ýnþaat, Ýnce Ýþler, Elektrik
        public string Telefon { get; set; }
        public string FaaliyetBolgesi { get; set; }
        public decimal BeklenenGunlukYevmiye { get; set; } // Ustanýn kendi günlük beklentisi
        public double Puan { get; set; } // Performans/Yýldýz
        public bool AktifMi { get; set; } = true;

        public ICollection<LaborFeasibility> Ýhaleler { get; set; }
    }
}

