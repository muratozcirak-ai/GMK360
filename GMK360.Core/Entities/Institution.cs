using System;
using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities
{
    public enum InstitutionType
    {
        Electricity = 1,
        Water = 2,
        NaturalGas = 3,
        Internet = 4,
        Telecom = 5,
        Municipality = 6, // Belediye (Emlak Vergisi, ÇTV)
        Other = 99
    }

    public class Institution : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Örn: "TEDAŞ", "İGDAŞ", "İSKİ"

        [Required]
        [MaxLength(50)]
        public string NormalizedCode { get; set; } // Örn: "tedas", "igdas" - API ve eşleştirme için küçük harf, boşluksuz

        public InstitutionType InstitutionType { get; set; }

        public bool IsApiSupported { get; set; } = false; // İleride API ile otomatik fatura çekilebilir mi?

        [MaxLength(255)]
        public string? LogoUrl { get; set; } // Kurumun logosu
    }
}
