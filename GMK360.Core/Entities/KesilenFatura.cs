using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public enum FaturaTipi
    {
        EFatura,
        EArsiv
    }

    public enum FaturaDurumu
    {
        Taslak,
        Gonderildi,
        Basarili,
        Hata
    }

    public class KesilenFatura : BaseEntity
    {
        public string FaturaNo { get; set; } // Örn: EML2024000000001
        public Guid ETTN { get; set; } // Benzersiz Fatura ID
        public string AliciUnvan { get; set; }
        public string AliciVKN_TCKN { get; set; }
        public decimal ToplamTutar { get; set; }
        public FaturaTipi FaturaTipi { get; set; } // EFatura veya EArsiv
        public FaturaDurumu Durum { get; set; } // Gönderildi, Başarılı, Hata vb.
        public DateTime Tarih { get; set; } = DateTime.UtcNow;

        // Faturanın kime kesildiğini uygulamadaki kullanıcı tablosuyla ilişkilendirmek isterseniz:
        public string? ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
