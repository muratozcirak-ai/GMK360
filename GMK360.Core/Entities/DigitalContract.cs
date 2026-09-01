using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public enum ContractType
    {
        Rental,               // Kira Sözleşmesi
        SalesCommitment,      // Satış Vaadi / Kapora Sözleşmesi
        AgencyAuthorization,  // Emlakçı Satış/Kiralama Yetki Belgesi
        PropertyShowing,      // Yer Gösterme Belgesi
        TradesmanService      // Usta / Hizmet Sözleşmesi
    }

    public enum ContractStatus
    {
        Draft,                // Taslak (Henüz Onaylanmadı)
        Active,               // Aktif (Yürürlükte)
        Expired,              // Süresi Bitti
        Terminated            // Feshedildi / İptal
    }

    public class DigitalContract : BaseEntity
    {
        public ContractType Type { get; set; }
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        // İlgili Mülk (Opsiyonel olabilir, örn genel bir yer gösterme ise)
        public int? PropertyId { get; set; }
        public virtual Property Property { get; set; }

        public int? RenovationRequestId { get; set; }
        public virtual RenovationRequest RenovationRequest { get; set; }

        // Sözleşmeyi Oluşturan Kişi (Emlakçı veya Mülk Sahibi)
        public string CreatorUserId { get; set; } = null!;
        public virtual ApplicationUser CreatorUser { get; set; } = null!;

        // İkinci Taraf (Kiracı, Alıcı veya Asıl Mülk Sahibi)
        // Platform üyesi ise User Id, değilse manuel metin
        public string? SecondPartyUserId { get; set; }
        public virtual ApplicationUser SecondPartyUser { get; set; }
        
        public string? SecondPartyFullName { get; set; }
        public string? SecondPartyIdentityNo { get; set; } // TCKN veya Vergi No
        public string? SecondPartyPhone { get; set; }

        // Sözleşme Tarihleri
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Finansal Veriler (Kira bedeli, kapora tutarı vs. JSON veya düz metin tutulabilir)
        public decimal? Amount { get; set; }
        public string? Currency { get; set; } = "TRY";

        // Dijital İz ve Belge
        public string? DocumentUrl { get; set; } // Üretilen PDF'in linki
        public string? CreatorIpAddress { get; set; }
        public string? SecondPartyIpAddress { get; set; }
        public DateTime? SignedAt { get; set; } // E-İmza veya SMS Onay tarihi
    }
}
