using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using GMK360.Core.Interfaces;
using GMK360.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GMK360.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly MailSettings _mailSettings;

        public EmailService(ILogger<EmailService> logger, IOptions<MailSettings> mailSettings)
        {
            _logger = logger;
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(_mailSettings.Host))
            {
                _logger.LogWarning("MailSettings is not configured. Simulating email to {To}", to);
                return;
            }

            try
            {
                using var client = new SmtpClient(_mailSettings.Host, _mailSettings.Port)
                {
                    Credentials = new NetworkCredential(_mailSettings.Mail, _mailSettings.Password),
                    EnableSsl = true
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_mailSettings.Mail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email successfully sent to {To} with subject {Subject}", to, subject);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending email to {To}", to);
                // Geliştirme aşamasında patlamaması için hatayı yutuyoruz (Anayasa kuralı 2: Asla patlama)
            }
        }
    }
}
