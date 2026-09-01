using System;

namespace GMK360.Core.Entities
{
    public class PropertyPayment : BaseEntity
    {
        public int PropertyId { get; set; } // MulkId
        public int LiabilityTypeId { get; set; } // YukumlulukTipId
        public string Period { get; set; } = null!; // Donem (Örn: 2026/Mayıs)
        public decimal Amount { get; set; } // Tutar
        public DateTime DueDate { get; set; } // SonOdemeTarihi
        public bool IsPaid { get; set; } // OdendiMi
        public DateTime? PaymentDate { get; set; } // OdemeTarihi
        public string? Description { get; set; } // Aciklama (Örn: İGDAŞ faturası)

        public virtual Property Property { get; set; } = null!;
        public virtual LiabilityType LiabilityType { get; set; } = null!;
    }
}
