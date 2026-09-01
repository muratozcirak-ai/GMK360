using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class Contract : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } // Örn: KVKK Aydınlatma Metni, Genel Hizmet Sözleşmesi

        [Required]
        public string ContentHtml { get; set; } // Sözleşme İçeriği

        [Required]
        [MaxLength(50)]
        public string ContractType { get; set; } // Örn: KVKK, SERVICE, AGENCY_AGREEMENT, TRADESMAN_AGREEMENT

        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsMandatory { get; set; } = true;

        public int Version { get; set; } = 1; // Sözleşme güncellendikçe artar
    }
}
