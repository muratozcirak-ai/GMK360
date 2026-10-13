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

        // Her gÃ¼n saat 15:30'da kurlarÄ±n gÃ¼ncellenmesi iÃ§in (Kaba bir delay ile, Cron job yerine)
        // Ya da basitÃ§e gÃ¼nde 1 kez Ã§alÄ±ÅŸmasÄ± iÃ§in.
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
            await Task.Yield(); while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await FetchAndSaveRatesAsync(stoppingToken);
                }
                catch (OperationCanceledException) { break; } catch (Exception ex) { try { _logger.LogError(ex, "TCMB kurlarÄ± Ã§ekilirken bir hata oluÅŸtu."); } catch { } }

                // 12 saat bekle
                try { await Task.Delay(_checkInterval, stoppingToken); } catch (OperationCanceledException) { break; }
            }
        }

        private async Task FetchAndSaveRatesAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TCMB kur gÃ¼ncellemesi baÅŸlatÄ±lÄ±yor...");

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://www.tcmb.gov.tr/kurlar/today.xml", stoppingToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("TCMB XML servisine eriÅŸilemedi.");
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
                    // VeritabanÄ±na yaz
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
                            _logger.LogInformation($"BugÃ¼nÃ¼n kurlarÄ± gÃ¼ncellendi: USD: {usdRate}, EUR: {eurRate}");
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
                            _logger.LogInformation($"Yeni gÃ¼nÃ¼n kurlarÄ± eklendi: USD: {usdRate}, EUR: {eurRate}");
                        }

                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
            }
        }
    }
}


