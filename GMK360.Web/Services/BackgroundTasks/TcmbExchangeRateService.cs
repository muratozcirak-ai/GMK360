using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GMK360.Web.Services.BackgroundTasks
{
    public class TcmbExchangeRateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TcmbExchangeRateService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        // Her gün saat 15:30'da kurların güncellenmesi için (Kaba bir delay ile, Cron job yerine)
        // Ya da basitçe günde 1 kez çalışması için.
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(12);

        public TcmbExchangeRateService(
            IServiceProvider serviceProvider, 
            ILogger<TcmbExchangeRateService> logger, 
            IHttpClientFactory httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await FetchAndSaveRatesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "TCMB kurları çekilirken bir hata oluştu.");
                }

                // 12 saat bekle
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task FetchAndSaveRatesAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TCMB kur güncellemesi başlatılıyor...");

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://www.tcmb.gov.tr/kurlar/today.xml", stoppingToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("TCMB XML servisine erişilemedi.");
                return;
            }

            var xmlContent = await response.Content.ReadAsStringAsync(stoppingToken);
            var doc = XDocument.Parse(xmlContent);

            var usdNode = doc.Descendants("Currency").FirstOrDefault(x => (string)x.Attribute("CurrencyCode") == "USD");
            var eurNode = doc.Descendants("Currency").FirstOrDefault(x => (string)x.Attribute("CurrencyCode") == "EUR");

            if (usdNode != null && eurNode != null)
            {
                var usdForexSelling = usdNode.Element("ForexSelling")?.Value;
                var eurForexSelling = eurNode.Element("ForexSelling")?.Value;

                var cultureInfo = new CultureInfo("en-US"); // TCMB XML uses dot for decimals
                
                if (decimal.TryParse(usdForexSelling, NumberStyles.Any, cultureInfo, out decimal usdRate) &&
                    decimal.TryParse(eurForexSelling, NumberStyles.Any, cultureInfo, out decimal eurRate))
                {
                    // Veritabanına yaz
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        
                        var today = DateTime.Today;
                        var existingRate = await dbContext.ExchangeRates
                            .FirstOrDefaultAsync(r => r.Date == today, stoppingToken);

                        if (existingRate != null)
                        {
                            existingRate.UsdRate = usdRate;
                            existingRate.EurRate = eurRate;
                            existingRate.UpdatedAt = DateTime.Now;
                            _logger.LogInformation($"Bugünün kurları güncellendi: USD: {usdRate}, EUR: {eurRate}");
                        }
                        else
                        {
                            var newRate = new ExchangeRate
                            {
                                Date = today,
                                UsdRate = usdRate,
                                EurRate = eurRate,
                                CreatedAt = DateTime.Now
                            };
                            dbContext.ExchangeRates.Add(newRate);
                            _logger.LogInformation($"Yeni günün kurları eklendi: USD: {usdRate}, EUR: {eurRate}");
                        }

                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
            }
        }
    }
}
