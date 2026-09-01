using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GMK360.Web.Services
{
    public class TempMediaCleanupService : BackgroundService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<TempMediaCleanupService> _logger;

        public TempMediaCleanupService(IWebHostEnvironment env, ILogger<TempMediaCleanupService> logger)
        {
            _env = env;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Temp Media Cleanup Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    CleanupTempFiles();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing Temp Media Cleanup.");
                }

                // Run every 24 hours
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("Temp Media Cleanup Service is stopping.");
        }

        private void CleanupTempFiles()
        {
            var tempFolder = Path.Combine(_env.WebRootPath, "uploads", "temp");
            if (!Directory.Exists(tempFolder)) return;

            var files = Directory.GetFiles(tempFolder);
            var threshold = DateTime.UtcNow.AddDays(-1);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTimeUtc < threshold)
                {
                    try
                    {
                        fileInfo.Delete();
                        _logger.LogInformation($"Deleted orphaned temp file: {fileInfo.Name}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to delete temp file: {fileInfo.Name}");
                    }
                }
            }
        }
    }
}
