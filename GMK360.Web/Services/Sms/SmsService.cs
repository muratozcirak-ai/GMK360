using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using GMK360.Core.Settings;
using Microsoft.Extensions.Logging;

namespace GMK360.Web.Services.Sms
{
    public class SmsService : ISmsService
    {
        private readonly SmsSettings _settings;
        private readonly ILogger<SmsService> _logger;
        private readonly HttpClient _httpClient;

        public SmsService(IOptions<SmsSettings> options, ILogger<SmsService> logger, HttpClient httpClient)
        {
            _settings = options.Value;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<string> SmsGonderAsync(string telefonNo, string mesajMetni)
        {
            try
            {
                // MOCK MODU: API Kullanıcı adı veya şifre boşsa, konsola log bas ve başarılı dön.
                if (string.IsNullOrWhiteSpace(_settings.Username) || string.IsNullOrWhiteSpace(_settings.Password))
                {
                    _logger.LogInformation($"[MOCK SMS GÖNDERİMİ] Alıcı: {telefonNo}, Mesaj: {mesajMetni}");
                    return "Başarılı: (MOCK) SMS loglandı.";
                }

                string jsonVeri = $@"{{
                    ""username"": ""{_settings.Username}"",
                    ""password"": ""{_settings.Password}"",
                    ""header"": ""{_settings.SenderId}"",
                    ""numbers"": [""{telefonNo}""],
                    ""message"": ""{mesajMetni}""
                }}";

                var icerik = new StringContent(jsonVeri, Encoding.UTF8, "application/json");

                HttpResponseMessage cevap = await _httpClient.PostAsync(_settings.ApiUrl, icerik);

                if (cevap.IsSuccessStatusCode)
                {
                    return "Başarılı: SMS gönderildi.";
                }
                else
                {
                    var responseBody = await cevap.Content.ReadAsStringAsync();
                    _logger.LogError($"SMS API Hatası: {cevap.StatusCode}, Detay: {responseBody}");
                    return $"Hata: Gönderim başarısız. Hata Kodu: {cevap.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS Gönderim Hatası");
                return $"Sistem Hatası: {ex.Message}";
            }
        }
    }
}
