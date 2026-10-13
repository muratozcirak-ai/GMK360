using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class DnaProfession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } // Örn: Duvarcı, Kalıpçı, Demirci
    }
}