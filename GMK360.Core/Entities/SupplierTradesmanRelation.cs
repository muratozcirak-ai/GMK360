using System.Collections.Generic;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class SupplierTradesmanRelation : BaseEntity
    {
        // Esnaf (Supplier) Kullanıcısı
        public string SupplierUserId { get; set; }
        public ApplicationUser SupplierUser { get; set; }

        // Usta (Tradesman) Kullanıcısı
        public string TradesmanUserId { get; set; }
        public ApplicationUser TradesmanUser { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
