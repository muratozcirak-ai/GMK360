using System;

namespace GMK360.Core.Entities
{
    public enum RenovationOfferStatus
    {
        Pending = 1,
        Accepted = 2,
        Rejected = 3
    }

    public class RenovationOffer : BaseEntity
    {
        public int RenovationRequestId { get; set; }
        public RenovationRequest RenovationRequest { get; set; }

        public int? ServiceProviderId { get; set; } // Sistem içi usta ise dolu olur
        public ServiceProvider ServiceProvider { get; set; }

        public string ExternalProviderName { get; set; } // Mahalledeki usta (sistem dışı manuel eklenen teklif)

        public decimal Price { get; set; }
        public string Notes { get; set; } // "Malzeme dahil", "Sadece işçilik" vb.

        public RenovationOfferStatus Status { get; set; } = RenovationOfferStatus.Pending;
    }
}
