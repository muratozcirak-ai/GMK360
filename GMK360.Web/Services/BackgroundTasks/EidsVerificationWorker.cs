using GMK360.Data;
using GMK360.Core.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Services.BackgroundTasks
{
    public class EidsVerificationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EidsVerificationWorker> _logger;

        public EidsVerificationWorker(IServiceProvider serviceProvider, ILogger<EidsVerificationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EIDS Verification Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckExpiredAuthorizationsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during EIDS verification check.");
                }

                // Her gece 03:00'te çalışması için 24 saat bekle (basit implementasyon)
                // Daha gelişmiş bir yapı için Quartz.NET veya Hangfire kullanılabilir.
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task CheckExpiredAuthorizationsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GMK360.Data.Contexts.ApplicationDbContext>();

            var now = DateTime.Now;

            // Yetkisi bitmiş aktif ilanları bul
            var expiredListings = await context.Properties
                .Where(p => p.State == ListingState.Active 
                         && p.HasAuthorization 
                         && p.AuthorizationEndDate.HasValue 
                         && p.AuthorizationEndDate.Value < now)
                .ToListAsync(stoppingToken);

            if (expiredListings.Any())
            {
                _logger.LogInformation($"Found {expiredListings.Count} listings with expired EIDS authorization. Revoking...");

                foreach (var listing in expiredListings)
                {
                    listing.State = ListingState.Revoked; // Yetki bittiği için otomatik iptal
                }

                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
