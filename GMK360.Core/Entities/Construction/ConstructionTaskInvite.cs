using System;

namespace GMK360.Core.Entities.Construction
{
    public class ConstructionTaskInvite : BaseEntity
    {
        public int ConstructionTaskId { get; set; }
        public ConstructionTask ConstructionTask { get; set; }

        public string CompanyName { get; set; }
        public string PhoneNumber { get; set; }
        
        public string Note { get; set; } // Müteahhitin taşerona yazdığı kısa davet notu

        // 0 = Bekliyor, 1 = Teklif Geldi, 2 = Reddedildi, 3 = Kabul Edildi (Taşeron atandı)
        public int Status { get; set; } 

        public decimal? QuoteAmount { get; set; } // Taşeron sisteme girip teklif verirse
        public string QuoteNote { get; set; } // Taşeronun fiyatla birlikte yazdığı not
        
        public DateTime InviteDate { get; set; } = DateTime.UtcNow;
        public DateTime? ResponseDate { get; set; }

        public string SentByUserId { get; set; } // Daveti atan müteahhit
        public string MatchedServiceProviderId { get; set; } // Firma sisteme üye olduğunda eşleşecek ID
    }
}
