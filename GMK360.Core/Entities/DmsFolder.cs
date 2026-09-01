using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    public class DmsFolder : BaseEntity
    {
        public int ShelfId { get; set; }
        public DmsShelf Shelf { get; set; }

        public string Name { get; set; } // Örn: Kadıköy Daire 5 Faturaları
        public string Description { get; set; }

        public ICollection<DmsDocument> Documents { get; set; }
    }
}
