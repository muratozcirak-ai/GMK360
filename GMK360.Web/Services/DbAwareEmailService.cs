using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Interfaces;
using GMK360.Core.Services;
using GMK360.Core.Settings;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GMK360.Web.Services
{
    /// <summary>
    /// Veritabanında tanımlı aktif SMTP ayarını (SystemEmailSettings) appsettings fallback'i ile birleştirerek e-posta gönderir.
    /// </summary>
    public class DbAwareEmailService : IEmailService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOptions<MailSettings> _fallbackOptions;
        private readonly ILogger<DbAwareEmailService> _logger;

        public DbAwareEmailService(
            ApplicationDbContext context,
            IOptions<MailSettings> fallbackOptions,
            ILogger<DbAwareEmailService> logger)
        {
            _context = context;
            _fallbackOptions = fallbackOptions;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            await SendEmailWithResultAsync(to, subject, body, null);
        }

        public async Task<EmailSendResult> SendEmailWithResultAsync(string to, string subject, string body, MailSettings? overrideSettings = null)
        {
            if (overrideSettings != null)
            {
                return await EmailService.SendWithSettingsAsync(overrideSettings, to, subject, body, _logger);
            }

            var fb = _fallbackOptions.Value ?? new MailSettings();
            var dbSetting = await _context.SystemEmailSettings
                .AsNoTracking()
                .Where(s => s.IsActive && !s.IsDeleted)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync();

            var effective = dbSetting != null
                ? new MailSettings
                {
                    Host = !string.IsNullOrWhiteSpace(dbSetting.Host) ? dbSetting.Host : fb.Host,
                    Port = dbSetting.Port > 0 ? dbSetting.Port : (fb.Port > 0 ? fb.Port : 587),
                    Mail = !string.IsNullOrWhiteSpace(dbSetting.Mail) ? dbSetting.Mail : fb.Mail,
                    Password = !string.IsNullOrWhiteSpace(dbSetting.Password) ? dbSetting.Password : fb.Password,
                    SenderName = !string.IsNullOrWhiteSpace(dbSetting.SenderName) ? dbSetting.SenderName : fb.SenderName,
                    EnableSsl = dbSetting.EnableSsl,
                    IgnoreCertificateErrors = dbSetting.IgnoreCertificateErrors
                }
                : fb;

            return await EmailService.SendWithSettingsAsync(effective, to, subject, body, _logger);
        }
    }
}
