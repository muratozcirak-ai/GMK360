using System;
using System.Collections.Generic;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public enum RenovationBiddingType
    {
        OpenBidding = 1,    // Herkese Açık
        AreaRestricted = 2, // Bölge Kısıtlamalı
        InviteOnly = 3      // Özel Davet (Seçilen ustalara)
    }

    public enum RenovationStatus
    {
        Open = 1,      // Tekliflere Açık
        Closed = 2,    // Teklif Alımına Kapatıldı (İhale bitti)
        Completed = 3, // İş tamamlandı
        Cancelled = 4, // İptal edildi
        Assigned = 5   // Biri atandı / İş üstlenildi
    }

    public class RenovationRequest : BaseEntity
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int? PropertyId { get; set; } // Hangi ev/mülk için isteniyor? (Bireysel)
        public Property Property { get; set; }
        
        public int? HousingComplexId { get; set; } // Hangi site için isteniyor?
        public HousingComplex HousingComplex { get; set; }

        public int? BuildingId { get; set; } // Hangi apartman blok için isteniyor?
        public Building Building { get; set; }
        
        public ExpenseScopeType ScopeType { get; set; } = ExpenseScopeType.Unit;
        public bool IsCommonArea { get; set; } = false; // Ortak alan ihalesi mi?

        public string Title { get; set; } // Örn: "2 oda boyanacak"
        public string Description { get; set; }
        
        public string PhotoUrl { get; set; } // Örnek fayans/boya fotoğrafları vs.
        
        // İhale Tipi ve Dağıtımı
        public RenovationBiddingType BiddingType { get; set; } = RenovationBiddingType.OpenBidding;
        
        // Bölge Kısıtlamalı (AreaRestricted) ise doldurulacak alanlar
        public int? TargetCityId { get; set; }
        public string? TargetDistrictIds { get; set; } // Virgülle ayrılmış string (Örn: "34,35,36")

        public RenovationStatus Status { get; set; } = RenovationStatus.Open;

        // B2B Atama / Manuel İş
        public string? AssigneeUserId { get; set; } // İşin atandığı Usta (Tradesman)
        public ApplicationUser AssigneeUser { get; set; }

        public string? AssignedSupplierId { get; set; } // İşi alan Esnaf (Eğer esnafa atandıysa)
        public ApplicationUser AssignedSupplier { get; set; }

        public bool IsManualJob { get; set; } = false; // Sistem dışından bulunan işler için

        public virtual ICollection<RenovationOffer> Offers { get; set; } = new List<RenovationOffer>();
        
        // Özel Davet (InviteOnly) Seçildiyse Atılan Davetler
        public virtual ICollection<RenovationRequestInvite> Invites { get; set; } = new List<RenovationRequestInvite>();
    }
}
