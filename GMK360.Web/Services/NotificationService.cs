using System;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;
using GMK360.Data.Contexts;

namespace GMK360.Web.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> SendEmailAsync(string emailAddress, string subject, string body, string? userId = null)
        {
            var log = new SystemNotificationLog
            {
                ToUserId = userId,
                EmailAddress = emailAddress,
                Type = NotificationType.Email,
                Subject = subject,
                MessageBody = body,
                IsDelivered = true, // Mock (Simulated)
                SentAt = DateTime.UtcNow,
                ProviderResponse = "MockEmailProvider: Success"
            };

            _db.SystemNotificationLogs.Add(log);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SendPushNotificationAsync(string userId, string title, string message)
        {
            var log = new SystemNotificationLog
            {
                ToUserId = userId,
                Type = NotificationType.PushNotification,
                Subject = title,
                MessageBody = message,
                IsDelivered = true, // Mock (Simulated)
                SentAt = DateTime.UtcNow,
                ProviderResponse = "MockPushProvider: Success"
            };

            _db.SystemNotificationLogs.Add(log);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message, string? userId = null)
        {
            var log = new SystemNotificationLog
            {
                ToUserId = userId,
                PhoneNumber = phoneNumber,
                Type = NotificationType.Sms,
                Subject = "SMS",
                MessageBody = message,
                IsDelivered = true, // Mock (Simulated)
                SentAt = DateTime.UtcNow,
                ProviderResponse = "NetGSMMock: Success"
            };

            _db.SystemNotificationLogs.Add(log);
            await _db.SaveChangesAsync();

            return true;
        }
    }
}
