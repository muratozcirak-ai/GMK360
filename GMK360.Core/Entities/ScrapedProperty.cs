using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class ScrapedProperty
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? PriceText { get; set; }
        public string? GrossAreaText { get; set; }
        public string? NetAreaText { get; set; }
        public string? RoomCount { get; set; }
        public string? BuildingAge { get; set; }
        public string? FloorNumberText { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? Platform { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; } = false;
    }
}
