using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    // FAVORİLER VE İZOLE AJANDA (Mülke Özel Gizli Notlar)
    public class FavoriteProperty : BaseEntity
    {
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int PropertyId { get; set; }
        public virtual Property Property { get; set; }

        // Bireysel kullanıcının ilana aldığı gizli not (Emlakçı göremez)
        public string? PrivateNote { get; set; } 
    }

    // TEKLİF TAKİP SİSTEMİ
    public class PropertyOffer : BaseEntity
    {
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int PropertyId { get; set; }
        public virtual Property Property { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OfferAmount { get; set; } // Verilen Fiyat Teklifi
        
        public string? Note { get; set; } // "Nakit ödeyeceğim" vb.
        
        // 1=Bekliyor, 2=Onaylandı, 3=Reddedildi
        public int Status { get; set; } = 1; 
    }
}
