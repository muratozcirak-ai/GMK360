using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities.B2b
{
    public class B2bBranch : BaseEntity
    {
        public int B2bCompanyId { get; set; }
        public B2bCompany B2bCompany { get; set; }

        [Required]
        [MaxLength(100)]
        public string BranchName { get; set; } // Merkez Ofis, Gebze Depo vb.

        [MaxLength(500)]
        public string? Address { get; set; }
        
        public string? City { get; set; }
        public string? District { get; set; }
        
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        
        public string? GoogleMapsUrl { get; set; }

        [MaxLength(20)]
        public string? LandlinePhone { get; set; }
    }
}
