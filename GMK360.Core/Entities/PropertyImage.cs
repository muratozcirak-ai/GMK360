namespace GMK360.Core.Entities
{
    public class PropertyImage : BaseEntity
    {
        public int PropertyId { get; set; }
        public Property Property { get; set; }
        
        public string ImageUrl { get; set; }
        public bool IsCover { get; set; } // Kapak fotoğrafı mı?
        
        public string? RoomTag { get; set; } // Örn: "Banyo", "Salon" (Null olabilir!)
        public int SortOrder { get; set; } // Kapak fotoğrafı ve sıralama için
        public bool IsVideo { get; set; } // Bu dosya bir video mu?
    }
}
