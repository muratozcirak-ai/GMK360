using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    public class Partner : BaseEntity
    {
        public int CompanyId { get; set; } // FirmaID (Çoklu sistem izolasyonu için)
        public string FullNameOrCompanyName { get; set; }
        public string Phone { get; set; }
        public bool IsTaxpayer { get; set; } = true;
        public string TaxNumberOrTc { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        // İlişkiler
        public ICollection<PropertyReservation> Reservations { get; set; }
    }
}
