using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;

namespace GMK360.Web.Services
{
    public class DailyReminderService : BackgroundService
    {
        private readonly ILogger<DailyReminderService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DailyReminderService(ILogger<DailyReminderService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DailyReminderService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("DailyReminderService running at: {time}", DateTimeOffset.Now);

                try
                {
                    await CheckAndSendRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing DailyReminderService.");
                }

                // Normalde günde 1 kez (Örn: gece 02:00) çalışır.
                // Test ortamı için 24 saat bekleteceğiz.
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("DailyReminderService is stopping.");
        }

        private async Task CheckAndSendRemindersAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var targetDate = DateTime.Today.AddDays(3); // 3 gün sonraki ödemeler

            var upcomingRecords = await dbContext.PropertyFinancialRecords
                .Include(r => r.Property)
                    .ThenInclude(p => p.User)
                .Where(r => !r.IsCompleted && r.DueDate.Date == targetDate.Date)
                .ToListAsync(stoppingToken);

            foreach (var record in upcomingRecords)
            {
                var user = record.Property?.User;
                if (user == null) continue;

                // Kullanıcının aktif abonelik paketini bul
                var activeSubscription = await dbContext.UserSubscriptions
                    .Include(us => us.Package)
                    .Where(us => us.UserId == user.Id && us.IsActive && us.EndDate > DateTime.Now)
                    .FirstOrDefaultAsync(stoppingToken);

                bool hasSmsRight = activeSubscription?.Package?.HasSmsNotifications ?? false;

                if (hasSmsRight)
                {
                    _logger.LogInformation($"[SMS GÖNDERİLDİ] Sayın {user.FirstName}, {record.ExpenseCategory} ödemenizin son günü yaklaşıyor. Tutar: {record.TotalAmount} TL.");
                }
                else
                {
                    _logger.LogInformation($"[E-POSTA GÖNDERİLDİ] Sayın {user.FirstName}, {record.ExpenseCategory} ödemenizin son günü yaklaşıyor. Tutar: {record.TotalAmount} TL.");
                }
            }
        }
    }
}
