using System;

namespace GMK360.Core.Entities.B2B
{
    public class B2BQuoteItem : BaseEntity
    {
        public int B2BQuoteRequestId { get; set; }
        public B2BQuoteRequest QuoteRequest { get; set; }

        public int MaterialCatalogId { get; set; }
        public Construction.MaterialCatalog MaterialCatalog { get; set; }

        public decimal Quantity { get; set; }
        public string Description { get; set; }
    }
}
