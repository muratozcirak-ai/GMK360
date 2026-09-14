using System;
using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities.Construction
{
    public class AgencyPhonebook : BaseEntity
    {
        public int AgencyId { get; set; }
        public Agency Agency { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } // Ad Soyad veya Firma Adý

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        // 1=Usta/MaviYaka, 2=Taþeron Firma, 3=Malzeme Tedarikçisi
        public byte ContactType { get; set; }

        // Virgülle ayrýlmýþ yetenek/malzeme etiketleri (Boya, Sýva, Seramik vb.)
        [MaxLength(500)]
        public string? Tags { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsRegistered { get; set; } = false;

        // Sisteme katýlýrsa eþleþeceði asýl kullanýcýnýn ID'si
        public string? LinkedUserId { get; set; }
    }
}
