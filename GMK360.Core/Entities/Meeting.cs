using System;

namespace GMK360.Core.Entities
{
    public class Meeting
    {
        public int Id { get; set; }
        
        public int BuildingId { get; set; }
        public virtual Building Building { get; set; }

        public string Title { get; set; } // Örn: Çatı Yalıtımı Gündemi
        public DateTime MeetingDate { get; set; }
        public string Agenda { get; set; }
        
        public string? MinutesUrl { get; set; } // Karar tutanağı PDF/Resim
    }
}
