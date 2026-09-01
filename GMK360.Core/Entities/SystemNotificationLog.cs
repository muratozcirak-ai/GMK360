using System;

namespace GMK360.Core.Entities
{
    public enum NotificationType
    {
        Sms,
        Email,
        PushNotification
    }

    public class SystemNotificationLog : BaseEntity
    {
        public string? ToUserId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EmailAddress { get; set; }
        
        public NotificationType Type { get; set; }
        public string Subject { get; set; } // Genelde E-posta için, SMS için "SMS"
        public string MessageBody { get; set; }
        
        public bool IsDelivered { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string? ProviderResponse { get; set; } // Örn: NetGSM API Response
    }
}
