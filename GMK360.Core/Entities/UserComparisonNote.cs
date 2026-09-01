using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class UserComparisonNote : BaseEntity
    {
        public string UserId { get; set; } 
        public ApplicationUser User { get; set; }

        public string NoteCategory { get; set; } // "Property" veya "Tradesman"
        
        public int? RelatedPropertyId { get; set; } 
        public Property RelatedProperty { get; set; }

        public int? RelatedTradesmanId { get; set; } 
        public ServiceProvider RelatedTradesman { get; set; }

        public string ContactPerson { get; set; } // Görüştüğü Emlakçı/Usta Adı
        public decimal PriceGiven { get; set; } // Karşı tarafın teklifi
        public decimal PriceOffered { get; set; } // Kullanıcının verdiği teklif
        public string DeliveryTime { get; set; } // Teslim süresi
        public string PrivateNotes { get; set; } // Özel kıyaslama notu
    }
}
