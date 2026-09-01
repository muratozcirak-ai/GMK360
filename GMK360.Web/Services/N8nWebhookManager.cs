using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GMK360.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GMK360.Web.Services
{
    public class N8nWebhookManager : IWebhookService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<N8nWebhookManager> _logger;

        public N8nWebhookManager(HttpClient httpClient, IConfiguration configuration, ILogger<N8nWebhookManager> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        private async Task<bool> SendWebhookAsync(string webhookUrlConfigKey, object payload)
        {
            var webhookUrl = _configuration[$"N8nWebhooks:{webhookUrlConfigKey}"];
            
            if (string.IsNullOrEmpty(webhookUrl))
            {
                _logger.LogWarning($"Webhook URL not configured for {webhookUrlConfigKey}");
                return false;
            }

            try
            {
                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(webhookUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully sent webhook to {webhookUrlConfigKey}");
                    return true;
                }
                
                _logger.LogError($"Failed to send webhook to {webhookUrlConfigKey}. Status Code: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending webhook to {webhookUrlConfigKey}");
                return false;
            }
        }

        public async Task<bool> SendFaultReportWebhookAsync(object faultData)
        {
            return await SendWebhookAsync("FaultReportUrl", faultData);
        }

        public async Task<bool> SendValuationReportWebhookAsync(object valuationData)
        {
            return await SendWebhookAsync("ValuationReportUrl", valuationData);
        }

        public async Task<bool> SendProfessionalMatchWebhookAsync(object matchData)
        {
            return await SendWebhookAsync("ProfessionalMatchUrl", matchData);
        }
    }
}
