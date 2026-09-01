using System;

namespace GMK360.Core.Entities
{
    public class DigitalDocument : BaseEntity
    {
        public string RelatedTable { get; set; } = null!; // BagliTablo
        public int RelatedRecordId { get; set; } // BagliKayitId
        public string FilePath { get; set; } = null!; // DosyaYolu
        public string Extension { get; set; } = null!; // Uzanti
    }
}
