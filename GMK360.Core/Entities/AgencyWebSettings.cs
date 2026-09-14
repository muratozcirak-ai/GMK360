using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class AgencyWebSettings : BaseEntity
    {
        public int AgencyId { get; set; }
        [ForeignKey("AgencyId")]
        public virtual Agency Agency { get; set; }

        public string AboutUsHtml { get; set; }
        public string VisionHtml { get; set; }
        public string MissionHtml { get; set; }

        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string MapEmbedIframe { get; set; }

        public string InstagramUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }

        public bool IsMaintenanceMode { get; set; } = false;
    }
}
