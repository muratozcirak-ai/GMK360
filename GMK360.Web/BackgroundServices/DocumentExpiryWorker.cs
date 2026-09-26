﻿﻿﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Construction;

namespace GMK360.Web.BackgroundServices
{
    public class DocumentExpiryWorker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<DocumentExpiryWorker> _logger;

        public DocumentExpiryWorker(IServiceProvider services, ILogger<DocumentExpiryWorker> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DocumentExpiryWorker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckExpiriesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing DocumentExpiryWorker.");
                }

                // Run every 24 hours
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task CheckExpiriesAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;
            var thirtyDaysFromNow = now.AddDays(30);

            var expiringDocs = await context.ProjectLegalDocuments
                .Include(d => d.ConstructionProject)
                .ThenInclude(p => p.Assignments)
                .Where(d => d.CompletedDate.HasValue && d.CompletedDate.Value <= thirtyDaysFromNow && d.Status == "Tamamlandı")
                .ToListAsync(cancellationToken);

            int notifCount = 0;
            foreach (var doc in expiringDocs)
            {
                var daysLeft = (doc.CompletedDate.Value - now).TotalDays;
                
                // Sadece belirli gunlerde gonder (30, 7, 3, 2, 1, 0, -1...)
                if (daysLeft > 7 && (int)daysLeft != 30 && (int)daysLeft != 15)
                {
                    continue; // 30 ile 7 arasinda sadece 15 kaldiysa gonder.
                }

                string severity = daysLeft <= 7 ? "KIRMIZI" : "TURUNCU";
                
                var assignees = doc.ConstructionProject.Assignments.Where(a => a.IsActive).ToList();
                
                foreach(var assignee in assignees)
                {
                    var notification = new UserNotification
                    {
                        ApplicationUserId = assignee.UserId,
                        Title = $"[{severity}] Evrak Bitis Uyarisi: {doc.DocumentName}",
                        Message = $"{doc.ConstructionProject.Name} santiyesindeki '{doc.DocumentName}' adli evrakin bitmesine {(int)daysLeft} gun kaldi! (Bitis: {doc.CompletedDate.Value:dd.MM.yyyy})",
                        LinkUrl = $"/ConstructionProject/Details/{doc.ConstructionProjectId}#documents",
                        CreatedAt = DateTime.UtcNow
                    };
                    context.UserNotifications.Add(notification);
                    notifCount++;
                }
            }

            if (notifCount > 0)
            {
                await context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"Sent {notifCount} document expiry notifications.");
            }
        }
    }
}
