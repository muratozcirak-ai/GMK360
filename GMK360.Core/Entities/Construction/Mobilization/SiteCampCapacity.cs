using GMK360.Core.Entities.Auditing;

namespace GMK360.Core.Entities.Construction.Mobilization
{
    // Þantiye kamp kapasitesi ve lojistik altyapýsý (Dað baþý þantiyeleri için hayati tablo)
    public class SiteCampCapacity
    {
        public int Id { get; set; }
        public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;
        public int ProjectId { get; set; }
        public ConstructionProject Project { get; set; }

        public int MaksimumYatakKapasitesi { get; set; }
        public int DoluYatakSayisi { get; set; }
        
        public int MutfakYemekKapasitesi { get; set; } // Kaç kiþiye yemek çýkabilir?
        
        // Yaþam Kalitesi (Altýn Kural)
        public bool TvVarMi { get; set; }
        public bool InternetVarMi { get; set; }
        public int SicakSuKapasitesiKisi { get; set; } // Kaç kiþilik sýcak su var?
        
        // Lojistik Zekasý için hesaplanan amortisman (Günlük kiþi baþý gider)
        public decimal KisiBasiGunlukKampingMaliyeti { get; set; } // Yatak, Elektrik, Su, Isýnma vb. günlük masraf payý
        public decimal KisiBasiGunlukYemekMaliyeti { get; set; }
    }
}

