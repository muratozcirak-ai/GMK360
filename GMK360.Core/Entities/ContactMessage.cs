using System;

namespace GMK360.Core.Entities
{
    public class ContactMessage : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public bool IsReplied { get; set; } = false;
        public string? AIAutoReply { get; set; } // Gelecekteki otomatik yanit temeli
    }
}
