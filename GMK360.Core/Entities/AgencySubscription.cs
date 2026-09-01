using System;

namespace GMK360.Core.Entities
{
    public class AgencySubscription : BaseEntity
    {
        public int AgencyId { get; set; }
        public Agency Agency { get; set; }
        
        // Ödeme Detayları
        public DateTime PaymentDate { get; set; }
        public DateTime CoverageStartDate { get; set; } // Ödemenin kapsadığı başlangıç
        public DateTime CoverageEndDate { get; set; } // Ödemenin kapsadığı bitiş (Örn: 1 Aylık)
        
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } // Kredi Kartı, Havale, Elden
        public string Notes { get; set; }
        
        public bool IsPaid { get; set; }
    }
}
