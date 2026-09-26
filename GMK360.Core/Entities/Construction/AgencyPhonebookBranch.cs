using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities.Construction
{
    public class AgencyPhonebookBranch : BaseEntity
    {
        public int AgencyPhonebookId { get; set; }
        public AgencyPhonebook AgencyPhonebook { get; set; }

        [MaxLength(100)]
        public string BranchName { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? GoogleMapsUrl { get; set; }
    }
}
