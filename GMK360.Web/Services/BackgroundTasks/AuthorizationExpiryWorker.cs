using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Services.BackgroundTasks
{
    public class AuthorizationExpiryWorker : BackgroundService
    {
        private readonly ILogger<AuthorizationExpiryWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public AuthorizationExpiryWorker(ILogger<AuthorizationExpiryWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AuthorizationExpiryWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("AuthorizationExpiryWorker running at: {time}", DateTimeOffset.Now);

                try
                {
                    await ProcessExpirationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing authorization expirations.");
                }

                // Her gece 00:00'da çalışması için veya şimdilik her 1 saatte bir test için
                // Prod ortamında DateTime hesaplaması ile gece yarısına kadar Delay verilebilir.
                // Şimdilik 12 saatte bir çalıştırıyoruz.
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        private async Task ProcessExpirationsAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var now = DateTime.UtcNow;
                var warningThreshold = now.AddDays(7);

                // 1. Süresi Bitenleri (Expired) Yayından Kaldır
                var expiredProperties = await dbContext.Properties
                    .Where(p => p.State == ListingState.Active && p.AuthorizationEndDate.HasValue && p.AuthorizationEndDate.Value < now)
                    .ToListAsync();

                foreach (var property in expiredProperties)
                {
                    property.State = ListingState.Expired;
                    
                    // Danışmana Bildirim
                    dbContext.UserNotifications.Add(new UserNotification
                    {
                        ApplicationUserId = property.UserId,
                        Title = "İlan Süresi Doldu!",
                        Message = $"'{property.Title}' başlıklı ilanınızın yetki süresi dolduğu için otomatik olarak yayından kaldırılmıştır.",
                        CreatedAt = now,
                        IsRead = false
                    });

                    // Mülk Sahibine Bildirim
                    if (!string.IsNullOrEmpty(property.OwnerUserId))
                    {
                        dbContext.UserNotifications.Add(new UserNotification
                        {
                            ApplicationUserId = property.OwnerUserId,
                            Title = "İlan Süresi Doldu",
                            Message = $"'{property.Title}' başlıklı mülkünüzün emlakçı yetki süresi dolmuştur.",
                            CreatedAt = now,
                            IsRead = false
                        });
                    }

                    _logger.LogInformation($"Property {property.Id} set to Expired. AuthorizationEndDate: {property.AuthorizationEndDate}");
                }

                // 2. 7 Gün Kalanlara Uyarı Gönder
                // Not: Her gün uyarı atmamak için son 24 saat içinde bildirim atılmış mı diye kontrol edilebilir, 
                // ancak basitlik adına süresi "Tam" 7. güne denk gelenleri alıyoruz. (Veya Notification tablosuna yeni uyarı atıldı mı flag eklenebilir)
                var warningProperties = await dbContext.Properties
                    .Where(p => p.State == ListingState.Active && p.AuthorizationEndDate.HasValue 
                             && p.AuthorizationEndDate.Value.Date == warningThreshold.Date)
                    .ToListAsync();

                foreach (var property in warningProperties)
                {
                    // Danışmana Bildirim
                    dbContext.UserNotifications.Add(new UserNotification
                    {
                        ApplicationUserId = property.UserId,
                        Title = "Sözleşme Bitiş Uyarısı (7 Gün)",
                        Message = $"'{property.Title}' başlıklı ilanınızın yetki süresi 7 gün sonra dolacaktır.",
                        CreatedAt = now,
                        IsRead = false
                    });

                    // Mülk Sahibine Bildirim
                    if (!string.IsNullOrEmpty(property.OwnerUserId))
                    {
                        dbContext.UserNotifications.Add(new UserNotification
                        {
                            ApplicationUserId = property.OwnerUserId,
                            Title = "Sözleşme Bitiş Uyarısı",
                            Message = $"'{property.Title}' başlıklı mülkünüzün emlakçı yetki süresi 7 gün sonra dolacaktır.",
                            CreatedAt = now,
                            IsRead = false
                        });
                    }
                }

                if (expiredProperties.Any() || warningProperties.Any())
                {
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation($"Processed {expiredProperties.Count} expired properties and warned {warningProperties.Count} properties.");
                }
            }
        }
    }
}
