using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class BuildingAnnouncement : BaseEntity
    {
        public int BuildingId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } // Örn: Toplantı Kararı, Genel Duyuru

        [ForeignKey("BuildingId")]
        public Building Building { get; set; }
    }
}
