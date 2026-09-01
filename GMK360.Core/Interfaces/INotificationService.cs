using System.Threading.Tasks;

namespace GMK360.Core.Interfaces
{
    public interface INotificationService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message, string? userId = null);
        Task<bool> SendEmailAsync(string emailAddress, string subject, string body, string? userId = null);
        Task<bool> SendPushNotificationAsync(string userId, string title, string message);
    }
}
