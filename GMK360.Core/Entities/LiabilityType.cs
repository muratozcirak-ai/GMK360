using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    public class LiabilityType : BaseEntity
    {
        public string Name { get; set; } = null!; // TipAdi
        public string Category { get; set; } = null!; // Kategori (Vergi, Sigorta, Abonelik, Özel Gider)
        public bool IsSystemType { get; set; } // SistemTipiMi (1: Admin tanımlı sabit, 0: Kullanıcının özel kalemi)

        public virtual ICollection<PropertyPayment> Payments { get; set; } = new List<PropertyPayment>();
    }
}
