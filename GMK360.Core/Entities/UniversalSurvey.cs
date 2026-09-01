using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class UniversalSurvey : BaseEntity
    {
        public string TargetUserId { get; set; } // Değerlendirilen kişi (Usta, Esnaf, Yönetici)
        public ApplicationUser TargetUser { get; set; }

        public string SenderUserId { get; set; } // Değerlendiren kişi (Müşteri, İşveren)
        public ApplicationUser SenderUser { get; set; }

        // Hangi işlem için değerlendirildi (Opsiyonel, işleme özel takip için)
        public int? RenovationRequestId { get; set; }
        public RenovationRequest RenovationRequest { get; set; }

        public int Rating { get; set; } // 1-5 arası yıldız
        public string Comments { get; set; }

        public bool IsApprovedForPublic { get; set; } = false; // "Onaylı Referans" statüsü
    }
}
