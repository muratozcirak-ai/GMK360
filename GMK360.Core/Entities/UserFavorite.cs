using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class UserFavorite : BaseEntity
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public bool IsPriceDropNotified { get; set; } = false;
        public bool NotifyOnPriceDrop { get; set; } = false;
        public decimal SavedAtPrice { get; set; }

        // Eklenebilir: Müşterinin ilana özel düştüğü not
        public string Note { get; set; }
    }
}
