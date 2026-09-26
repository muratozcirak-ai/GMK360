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
        public string Name { get; set; } // Ad Soyad veya Firma Adı

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        // 1=Usta/MaviYaka, 2=Taşeron Firma, 3=Malzeme Tedarikçisi
        public byte ContactType { get; set; }

        // Virgülle ayrılmış yetenek/malzeme etiketleri (Boya, Sıva, Seramik vb.)
        [MaxLength(500)]
        public string? Tags { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsRegistered { get; set; } = false;

        // Sisteme katılırsa eşleşeceği asıl kullanıcının ID'si
        public string? LinkedUserId { get; set; }

        public virtual System.Collections.Generic.ICollection<AgencyPhonebookBranch> Branches { get; set; } = new System.Collections.Generic.List<AgencyPhonebookBranch>();
        public virtual System.Collections.Generic.ICollection<AgencyPhonebookContact> Contacts { get; set; } = new System.Collections.Generic.List<AgencyPhonebookContact>();

        // --- ADRES VE KONUM ---
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? GoogleMapsUrl { get; set; }

        // --- ILETISIM DETAYLARI (COKLU) ---
        public string? MobilePhone2 { get; set; } 
        public string? LandlinePhone { get; set; } 
        public string? ExtensionNumber { get; set; } 
        public string? WebsiteUrl { get; set; }

        // --- KURUMSAL BILGILER ---
        public string? AuthorizedPerson { get; set; } 
        public string? AuthorizedPersonRole { get; set; } 
        public string? TaxOffice { get; set; }
        public string? TaxNumber { get; set; }
        public string? Iban { get; set; }
    }
}
