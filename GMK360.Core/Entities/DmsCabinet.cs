using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    public class DmsCabinet : BaseEntity
    {
        public string Name { get; set; } // Örn: Mali Evraklar Dolabı, Sözleşmeler Dolabı
        public string Description { get; set; }
        
        // Bu dolap kimin/hangi kullanıcının?
        public string UserId { get; set; }

        public ICollection<DmsShelf> Shelves { get; set; }
    }
}
