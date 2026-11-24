using System;
using System.Threading.Tasks;
using GMK360.Core.Interfaces;
using GMK360.Core.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

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
            await SendEmailWithResultAsync(to, subject, body, null);
        }

        public Task<EmailSendResult> SendEmailWithResultAsync(string to, string subject, string body, MailSettings? overrideSettings = null)
        {
            var effective = overrideSettings ?? _mailSettings;
            return SendWithSettingsAsync(effective, to, subject, body, _logger);
        }

        public static async Task<EmailSendResult> SendWithSettingsAsync(
            MailSettings settings,
            string to,
            string subject,
            string body,
            ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                return new EmailSendResult { Success = false, Message = "Alıcı e-posta adresi boş olamaz." };
            }

            if (settings == null || string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.Mail))
            {
                logger.LogWarning("MailSettings eksik. Simülasyon: {To}", to);
                return new EmailSendResult { Success = false, Message = "SMTP sunucu adresi veya gönderici e-posta yapılandırılmamış." };
            }

            if (string.IsNullOrWhiteSpace(settings.Password))
            {
                logger.LogWarning("MailSettings şifresi boş. E-posta gönderilemedi: {To}", to);
                return new EmailSendResult { Success = false, Message = "SMTP e-posta şifresi girilmemiş (Admin > E-Posta Ayarları bölümünden kaydedin)." };
            }

            try
            {
                var message = new MimeMessage();
                var senderDisplay = string.IsNullOrWhiteSpace(settings.SenderName) ? "GMK360 Platform" : settings.SenderName.Trim();
                message.From.Add(new MailboxAddress(senderDisplay, settings.Mail.Trim()));
                message.To.Add(MailboxAddress.Parse(to.Trim()));
                message.Subject = subject ?? string.Empty;
                message.Body = new BodyBuilder { HtmlBody = body ?? string.Empty }.ToMessageBody();

                var socketOption = settings.Port == 465
                    ? SecureSocketOptions.SslOnConnect
                    : (settings.EnableSsl ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.None);

                using var client = new SmtpClient { Timeout = 15000 };
                if (settings.IgnoreCertificateErrors)
                {
                    client.ServerCertificateValidationCallback = (_, _, _, _) => true;
                }

                await client.ConnectAsync(settings.Host.Trim(), settings.Port > 0 ? settings.Port : 587, socketOption);
                await client.AuthenticateAsync(settings.Mail.Trim(), settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                logger.LogInformation("Email successfully sent to {To} with subject {Subject}", to, subject);
                return new EmailSendResult
                {
                    Success = true,
                    Message = $"E-posta '{to.Trim()}' adresine başarıyla gönderildi ({settings.Host}:{settings.Port})."
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while sending email to {To} via {Host}:{Port}", to, settings.Host, settings.Port);
                return new EmailSendResult
                {
                    Success = false,
                    Message = $"SMTP hatası ({settings.Host}:{settings.Port}): {ex.Message}"
                };
            }
        }
    }
}
