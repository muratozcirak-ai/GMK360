using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    public class DmsShelf : BaseEntity
    {
        public int CabinetId { get; set; }
        public DmsCabinet Cabinet { get; set; }

        public string Name { get; set; } // Örn: 2026 Yılı Tahsilatları, B Blok Sözleşmeleri
        public string Description { get; set; }

        public ICollection<DmsFolder> Folders { get; set; }
    }
}
