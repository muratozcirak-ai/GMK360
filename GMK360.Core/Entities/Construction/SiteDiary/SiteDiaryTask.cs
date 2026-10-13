using System;

namespace GMK360.Core.Entities.Construction.SiteDiary
{
    // 4. ÞANTÝYE GÜNLÜÐÜ (Ýþin Sahaya Ýnmiþ Hali)
    public class SiteDiaryTask : BaseEntity
    {
        public int ProjectId { get; set; }
        
        // Domino Etkisi Baðlantýsý: Hangi ana iþe baðlý (Örn: Phase 2 deki ScheduleTaskId)
        public int? BagliOlduguAnaGorevId { get; set; }
        
        public DateTime GorevTarihi { get; set; } 
        
        public string GorevTanimi { get; set; }
        public string GorevTipi { get; set; } // ""Aksiyon"" veya ""Checklist""
        
        // YAP DEÐÝL, YAPTIR
        public string ZimmetliPersonelId { get; set; } // Kime atandý?
        
        // Durum Takibi (True/False yerine Enum veya String)
        // Bekliyor, Devam Ediyor, Tamamlandi, Iptal, Ertelendi
        public string Durum { get; set; } = "Bekliyor"; 
        
        // Sorumluluk Reddi ve Kanýt
        public string KanitFotografUrl { get; set; } 
        public string OnaylayanKullaniciId { get; set; } // Ýþe kim onay verdi?

        public virtual ConstructionProject Project { get; set; }
    }
}

