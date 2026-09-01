using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class UserBillingInfo : BaseEntity
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string TaxNumber { get; set; } // Vergi Kimlik No / TCKN
        public string TaxOffice { get; set; } // Vergi Dairesi
        public string CompanyName { get; set; } // Fatura Ünvanı
        public string BillingAddress { get; set; } // Fatura Adresi
    }
}
