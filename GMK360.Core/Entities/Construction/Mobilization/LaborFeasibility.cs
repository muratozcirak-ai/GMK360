using GMK360.Core.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities.Construction.Mobilization
{
    // Ýþ Gücü Karar Odasý - (Yap mý Yaptýr mý analizi)
    public class LaborFeasibility
    {
        public int Id { get; set; }
        public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;
        public int ProjectId { get; set; }
        public ConstructionProject Project { get; set; }

        public string IsPaketiAdi { get; set; } // Örn: Kaba Ýnþaat
        public int HedefSureGun { get; set; }

        // A ÞIKKI: TAÞERON VERÝLERÝ
        public decimal? TaseronAnahtarTeslimFiyat { get; set; }
        
        // B ÞIKKI: KENDÝ EKÝBÝMÝZ (YEVMÝYE) VERÝLERÝ
        public int? SecilenUstaId { get; set; }
        public SubcontractorPool SecilenUsta { get; set; }
        public int UstaSayisi { get; set; }
        public decimal UstaYevmiyesi { get; set; }
        
        public int DuzIsciSayisi { get; set; }
        public decimal DuzIsciYevmiyesi { get; set; }

        // OTOMATÝK HESAPLANAN GERÇEK MALÝYETLER (SGK VE LOJÝSTÝK MOTORU)
        public decimal HesaplananCiplakYevmiyeMaliyeti { get; set; }
        public decimal HesaplananSgkVeVergiMaliyeti { get; set; }
        public decimal HesaplananKampVeYemekMaliyeti { get; set; }

        [NotMapped]
        public decimal ToplamGercekIsverenMaliyeti => HesaplananCiplakYevmiyeMaliyeti + HesaplananSgkVeVergiMaliyeti + HesaplananKampVeYemekMaliyeti;

        // KARAR
        public string SecilenYontem { get; set; } // 'Taseron' veya 'Yevmiye'
        public string OnaylayanKullaniciId { get; set; } // Kararý kim verdi?
    }
}

