using System;

namespace GMK360.Core.Entities.B2B
{
    public class B2BQuoteInviteItem : BaseEntity
    {
        public int B2BQuoteInviteId { get; set; }
        public B2BQuoteInvite QuoteInvite { get; set; }

        public int B2BQuoteItemId { get; set; }
        public B2BQuoteItem QuoteItem { get; set; }

        public decimal OfferedUnitPrice { get; set; }
        public bool IsVatIncluded { get; set; } = false;
        public decimal VatRate { get; set; } = 20; // Default 20%
        
        // Calculated field helper for display
        public decimal TotalPrice => OfferedUnitPrice * (QuoteItem?.Quantity ?? 0);
    }
}
