using System;

namespace GMK360.Core.Entities
{
    public enum OwnerType
    {
        Platform = 1,
        HousingComplex = 2,
        Building = 3,
        Agency = 4 // Emlak Şirketleri için
    }

    public class BankAccount : BaseEntity
    {
        public OwnerType OwnerType { get; set; }
        public int? OwnerId { get; set; } // HousingComplexId, BuildingId, AgencyId vb.

        public string BankName { get; set; } // Örn: Ziraat Bankası
        public string AccountName { get; set; } // Örn: İşletme Hesabı, Havuz Hesabı
        public string IBAN { get; set; }
        
        public decimal CurrentBalance { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
