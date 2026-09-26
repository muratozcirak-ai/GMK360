using System;

namespace GMK360.Core.Entities
{
    public class SystemDocumentDependency : BaseEntity
    {
        // Asıl alınmak istenen/hedeflenen evrak (Örn: Yıkım Ruhsatı)
        public int TargetDocumentId { get; set; }
        public SystemLegalDocumentTemplate TargetDocument { get; set; }

        // Önce tamamlanması ŞART olan ön koşul evrakı (Örn: Karot Raporu)
        public int PrerequisiteDocumentId { get; set; }
        public SystemLegalDocumentTemplate PrerequisiteDocument { get; set; }
    }
}
