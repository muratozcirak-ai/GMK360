using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace GMK360.Web.Services
{
    public class MockEmailSender : IEmailSender
    {
        private readonly ILogger<MockEmailSender> _logger;
        private readonly GMK360.Core.Interfaces.IEmailService _emailService;

        public MockEmailSender(ILogger<MockEmailSender> logger, GMK360.Core.Interfaces.IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation($"Identity Email Sender tetiklendi. Kime: {email}, Konu: {subject}");
            
            // Gerçek SMTP servisimizi çağırıyoruz
            await _emailService.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
