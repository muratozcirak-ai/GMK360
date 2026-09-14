using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GMK360.Core.Entities.Construction
{
    public class AgendaParticipant
    {
        public int Id { get; set; }
        public int AgendaRecordId { get; set; }
        
        [ForeignKey("AgendaRecordId")]
        [JsonIgnore]
        public virtual AgendaRecord AgendaRecord { get; set; }

        public int PhonebookId { get; set; }
        [ForeignKey("PhonebookId")]
        public virtual AgencyPhonebook Phonebook { get; set; }
        
        public string? UserId { get; set; } // Null ise Sistem Dışı Misafir, Dolu ise Gölge Kullanıcı
        
        public string? AccessToken { get; set; } // Misafirler için GUID token
        
        public string? ParticipantNotes { get; set; } // Katılımcının sisteme girdiği son not / itiraz / onay
        
        public bool IsViewed { get; set; } = false;
        public bool ReminderSent { get; set; } = false;
    }
}
