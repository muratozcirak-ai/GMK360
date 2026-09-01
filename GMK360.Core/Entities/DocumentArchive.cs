using System;

namespace GMK360.Core.Entities
{
    public class DocumentArchive
    {
        public int Id { get; set; }
        
        public int BuildingId { get; set; }
        public virtual Building Building { get; set; }

        public string Title { get; set; } // Asansör Raporu, SGK Bildirgesi vb.
        public string DocumentUrl { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
    }
}
