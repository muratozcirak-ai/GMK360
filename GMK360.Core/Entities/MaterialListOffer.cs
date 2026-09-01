using System;

namespace GMK360.Core.Entities
{
    public class MaterialListOffer : BaseEntity
    {
        public int MaterialListId { get; set; }
        public MaterialList MaterialList { get; set; }

        public int B2bSupplierId { get; set; }
        public B2bSupplier Supplier { get; set; }

        public decimal TotalPrice { get; set; }
        
        public DateTime ValidUntil { get; set; }
        public string Description { get; set; }
    }
}
