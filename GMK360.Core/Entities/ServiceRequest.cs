using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    public enum ServiceRequestStatus
    {
        Open,           // Teklife Açık
        OfferSelected,  // Teklif Seçildi / Usta Bekleniyor
        InProgress,     // İş Yapılıyor
        Completed,      // Tamamlandı
        Cancelled       // İptal Edildi
    }

    public enum ServiceRequestScope
    {
        Neighborhood,   // Sadece kendi mahallesi
        District,       // Tüm İlçe
        City            // Tüm Şehir (İl)
    }

    public class ServiceRequest : BaseEntity
    {
        public string UserId { get; set; } // Mülk Sahibi
        
        // Mülk ile tam bağlantı (Aidiyet)
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public int ServiceCategoryId { get; set; }
        public ServiceCategory ServiceCategory { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        
        // Medya/Fotoğraf (Hasarın fotoğrafı vb.)
        public string MediaUrl { get; set; }

        // Hedeflenen teklif verenlerin kapsamı (Sadece mahallemdeki ustalar vs)
        public ServiceRequestScope TargetScope { get; set; } = ServiceRequestScope.District;

        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Open;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ExpectedDate { get; set; } // İşin ne zaman yapılmasını istiyor?

        public ICollection<ServiceOffer> Offers { get; set; }
    }
}
