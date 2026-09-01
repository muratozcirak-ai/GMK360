using System;

namespace GMK360.Core.Entities
{
    public enum ServiceOfferStatus
    {
        Pending,   // Beklemede
        Accepted,  // Kabul Edildi
        Rejected,  // Reddedildi
        Withdrawn  // Usta tarafından geri çekildi
    }

    public class ServiceOffer : BaseEntity
    {
        public int ServiceRequestId { get; set; }
        public ServiceRequest ServiceRequest { get; set; }

        // Usta / Hizmet veren (UserId)
        public string ProviderUserId { get; set; }

        public decimal PriceAmount { get; set; }
        public string Message { get; set; } // Örn: "Yarın gelip 3 saatte hallederim. Malzeme hariç fiyattır."

        public int EstimatedDaysToComplete { get; set; } // Kaç günde tamamlar?

        public ServiceOfferStatus Status { get; set; } = ServiceOfferStatus.Pending;

        public DateTime OfferDate { get; set; } = DateTime.Now;
    }
}
