using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class SystemMeeting : BaseEntity
    {
        // Generic Context Fields (örneğin: "Building", "Agency")
        public string ContextType { get; set; } = "Building";
        public int ContextId { get; set; }

        [Required]
        public DateTime MeetingDate { get; set; }

        [Required]
        [MaxLength(300)]
        public string AgendaTitle { get; set; } // Örn: Yeni Yıl Bütçe Görüşmesi ve Çatı Yalıtımı

        public bool IsConcluded { get; set; } = false; // Toplantı tamamlandı mı?

        // Toplantıya bağlı kararlar
        public virtual ICollection<SystemMeetingDecision> Decisions { get; set; }
        
        // Toplantıya bağlı anketler (Survey/Poll)
        public virtual ICollection<MeetingSurvey> Surveys { get; set; }
    }
}
