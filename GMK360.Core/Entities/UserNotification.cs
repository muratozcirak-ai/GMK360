using GMK360.Core.Entities.Identity;
using System;

namespace GMK360.Core.Entities
{
    public class UserNotification : BaseEntity
    {
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        
        public string? LinkUrl { get; set; } // Bildirime tıklandığında gidilecek URL
    }
}
