using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using GMK360.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GMK360.Web.Services.AI
{
    public class GeminiAiManager : IGeminiAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiAiManager> _logger;
        private readonly string _apiKey;

        public GeminiAiManager(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiAiManager> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["GeminiApiKey"] ?? string.Empty;
        }

        private async Task<string> CallGeminiApiAsync(string systemInstruction, string userMessage)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("GeminiApiKey is missing in configuration.");
                return "{\"error\": \"GeminiApiKey eksik.\"}";
            }

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = systemInstruction + "\n\n" + userMessage }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.2,
                    responseMimeType = "application/json"
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

            try
            {
                var response = await _httpClient.PostAsync(requestUrl, jsonContent);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                
                // Basit bir parse işlemi: Yanıt içerisindeki text alanını döndür
                using var jsonDoc = JsonDocument.Parse(responseString);
                var root = jsonDoc.RootElement;
                
                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var content = candidates[0].GetProperty("content");
                    if (content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        return parts[0].GetProperty("text").GetString() ?? string.Empty;
                    }
                }
                
                return "{}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini API çağrısında hata oluştu.");
                return $"{{\"error\": \"{ex.Message}\"}}";
            }
        }

        public async Task<string> AnalyzeMaintenanceRequestAsync(string userMessage)
        {
            string systemPrompt = @"
Sen profesyonel bir gayrimenkul ve tesisat yöneticisisin. 
Aşağıdaki müşteri arıza bildirimini analiz et ve YALNIZCA geçerli bir JSON formatında şu bilgileri dön:
- Kategori (Örn: Tesisat, Elektrik, Beyaz Eşya, Mobilya, Genel Bakım vb.)
- Aciliyet (Yüksek, Orta, Düşük)
- UstaTipi (Su Tesisatçısı, Elektrikçi, Boyacı vb.)
- TahminiSorun (Sorunun ne olabileceğine dair 1 cümlelik özet)

Örnek Çıktı:
{
  ""Kategori"": ""Tesisat"",
  ""Aciliyet"": ""Yüksek"",
  ""UstaTipi"": ""Su Tesisatçısı"",
  ""TahminiSorun"": ""Kombi altındaki borularda su kaçağı var.""
}";

            return await CallGeminiApiAsync(systemPrompt, userMessage);
        }

        public async Task<string> EvaluateLeadMatchAsync(string leadRequirements, string unitDescription)
        {
            string systemPrompt = @"
Sen bir uzman emlak danışmanısın. Müşterinin talepleri ile mevcut dairenin özelliklerini karşılaştırarak ne kadar uygun olduğunu analiz et.
YALNIZCA geçerli bir JSON dön:
- EslesmeYuzdesi (0 ile 100 arasında bir sayı)
- OlumluYonler (Liste olarak, dairenin talebe uyan özellikleri)
- OlumsuzYonler (Liste olarak, dairenin talebi karşılamadığı noktalar)
- OzetDegerlendirme (1-2 cümlelik neden uygun veya değil yorumu)
";
            
            string combinedMessage = $"MÜŞTERİ TALEBİ:\n{leadRequirements}\n\nDAİRE ÖZELLİKLERİ:\n{unitDescription}";
            return await CallGeminiApiAsync(systemPrompt, combinedMessage);
        }

        public async Task<string> SummarizeContractAsync(string contractText)
        {
            string systemPrompt = @"
Aşağıda verilen hukuki kira veya hizmet sözleşmesinin en önemli kısımlarını özetle.
YALNIZCA geçerli bir JSON dön:
- SozlesmeSuresi (Örn: 1 Yıl, Belirsiz vb.)
- UcretVeOdemeSartlari (Özet olarak)
- CaymaBedeliVeSartlari (Özet olarak)
- KritikMaddeler (Liste halinde sözleşmedeki en önemli 3 madde)
";
            return await CallGeminiApiAsync(systemPrompt, contractText);
        }

        public async Task<string> AnalyzePropertyValuationAsync(GMK360.Core.DTOs.PropertyValuationRequestDto details)
        {
            string systemPrompt = @"
Sen uzman bir gayrimenkul değerleme uzmanısın. Kullanıcının verdiği mülk özelliklerini (şehir, ilçe, metrekare, oda sayısı vb.) analiz et.
Piyasa mantığıyla (varsayımsal ama tutarlı) tahmini bir fiyat aralığı belirle ve mülkün değerini etkileyen unsurları açıkla.
YALNIZCA geçerli bir JSON dön:
- MinFiyatTahmini (Sayısal değer, örn: 2500000)
- MaxFiyatTahmini (Sayısal değer, örn: 2800000)
- DegerlemeOzeti (1-2 cümlelik genel değerlendirme yorumu)
- Artilar (Liste halinde mülkün değerini artıran özellikler)
- Eksiler (Liste halinde mülkün değerini düşüren özellikler)
";
            string userMessage = JsonSerializer.Serialize(details);
            return await CallGeminiApiAsync(systemPrompt, userMessage);
        }

        public async Task<string> GenerateActionableFaultReportAsync(string issueDescription)
        {
            string systemPrompt = @"
Sen akıllı bir arıza/bakım yönetim asistanısın. Müşteri veya kiracının bildirdiği sorunu analiz et.
YALNIZCA geçerli bir JSON dön:
- Aciliyet (Yüksek, Orta, Düşük)
- IlkYardimTavsiyesi (Kullanıcıya veya yöneticiye anında alınması gereken önlem. Örn: 'Sigortayı indirin' veya 'Ana vanayı kapatın')
- UstaTipi (Su Tesisatçısı, Elektrikçi, Çilingir vb.)
- OlasıSebep (Arızanın tahmini kaynağı)
";
            return await CallGeminiApiAsync(systemPrompt, issueDescription);
        }

                public async Task<string> MatchProfessionalAsync(string userRequest, string professionalType)
        {
            string systemPrompt = $@"
Sen akıllı bir {professionalType} eşleştirme asistanısın. Kullanıcının talebini analiz edip, veri tabanımızdaki (sana bu metinle iletilmeyen ama hayali olarak puanlayacağın) uzmanlar için arama kriterleri ve eşleştirme stratejisi oluştur.
YALNIZCA geçerli bir JSON dön:
- OnerilenUzmanTipi (Örn: 'Ticari Gayrimenkul Uzmanı', 'Endüstriyel Elektrikçi' vb.)
- AnahtarKelimeler (Arama için kullanılacak kelimeler listesi)
- TalepOzeti (Kullanıcının ne istediğinin net 1 cümlelik özeti)
- EslesmeZorlugu (Kolay, Orta, Zor)
";
            return await CallGeminiApiAsync(systemPrompt, userRequest);
        }

        public async Task<string> AnalyzeNeighborhoodAsync(string districtName, string neighborhoodName)
        {
            // We use standard generation config but we will NOT ask for JSON this time, we want Markdown/HTML.
            // So we override the generic call or just do a direct call here.
            if (string.IsNullOrEmpty(_apiKey)) return "Sistemde yapay zeka anahtarı eksik.";

            string prompt = $@"
Sen Türkiye'nin en tecrübeli gayrimenkul yatırım ve bölge analiz uzmanısın.
Kullanıcı '{districtName} ilçesi, {neighborhoodName} mahallesi' için bir bölge analizi istiyor.
Lütfen bu mahalle hakkında şu başlıkları içeren kısa, profesyonel ve etkileyici bir Markdown raporu hazırla:
1. Genel Profil (Mahallenin sosyokültürel durumu, kimler yaşar?)
2. Ulaşım ve Konum (Merkeze yakınlık, metro/otobüs vs.)
3. Sosyal ve Sağlık Donatıları (Hastaneler, okullar, parklar, yaşam kalitesi)
4. Emlak ve Yatırım Potansiyeli (Kira talebi yüksek mi? Amortisman süresi nasıl? Yatırıma uygun mu?)

Not: Kesinlikle JSON dönme, doğrudan Markdown formatında şık bir metin dön.
";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { temperature = 0.4 } // text generation
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

            try
            {
                var response = await _httpClient.PostAsync(requestUrl, jsonContent);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                
                using var jsonDoc = JsonDocument.Parse(responseString);
                var root = jsonDoc.RootElement;
                
                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var content = candidates[0].GetProperty("content");
                    if (content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        return parts[0].GetProperty("text").GetString() ?? string.Empty;
                    }
                }
                
                return "Rapor şu an oluşturulamadı.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini Neighborhood Analysis Error");
                return "Bölge analizi geçici olarak kullanılamıyor.";
            }
        }
    }
}

