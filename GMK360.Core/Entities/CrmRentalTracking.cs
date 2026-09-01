using System;

namespace GMK360.Core.Entities
{
    public class CrmRentalTracking : BaseEntity
    {
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public int TenantId { get; set; }
        public CrmContact Tenant { get; set; }

        public int LandlordId { get; set; }
        public CrmContact Landlord { get; set; }

        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        
        public decimal MonthlyRentAmount { get; set; }
        public int PaymentDayOfMonth { get; set; } // Her ayın kaçıncı günü ödenecek (Örn: 5)
        
        public bool IsPaymentReceivedThisMonth { get; set; } // Bu ay ödeme alındı mı?
        public string Notes { get; set; }
    }
}
