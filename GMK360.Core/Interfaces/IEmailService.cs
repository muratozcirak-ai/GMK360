using System.Threading.Tasks;
using GMK360.Core.Settings;

namespace GMK360.Core.Interfaces
{
    public sealed class EmailSendResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task<EmailSendResult> SendEmailWithResultAsync(string to, string subject, string body, MailSettings? overrideSettings = null);
    }
}
