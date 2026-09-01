using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class ComplexBlock : BaseEntity
    {
        public int BuildingId { get; set; }
        public virtual Building Building { get; set; }
        
        [NotMapped]
        public int ComplexId { get => BuildingId; set => BuildingId = value; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Örn: "A Blok", "B Blok", "1. Giriş"

        [Required]
        public int TotalFloors { get; set; } // O bloğun kat sayısı
    }
}
