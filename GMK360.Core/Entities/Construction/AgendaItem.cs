using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GMK360.Core.Entities.Construction
{
    public class AgendaItem
    {
        public int Id { get; set; }
        
        public int AgendaRecordId { get; set; }
        [ForeignKey("AgendaRecordId")]
        [JsonIgnore]
        public virtual AgendaRecord AgendaRecord { get; set; }

        public int OrderNo { get; set; }
        public string TopicTitle { get; set; }
        public string PresentationText { get; set; }
        public string ImageUrl { get; set; }
        public string LiveMeetingNotes { get; set; }
    }
}
