using System;
using System.Text.Json;
using System.Threading.Tasks;
using GMK360.Core.Interfaces;
using Microsoft.Extensions.Logging;
// Google.Cloud.GenerativeAI kütüphanesi referanslarına göre uyarlanacaktır.

namespace GMK360.Web.Services.AI
{
    public class AiSupportAnalyzerService : IAiSupportAnalyzer
    {
        private readonly ILogger<AiSupportAnalyzerService> _logger;
        // private readonly IGeminiClient _geminiClient; (Projenizdeki Gemini istemcisi)

        public AiSupportAnalyzerService(ILogger<AiSupportAnalyzerService> logger)
        {
            _logger = logger;
        }

        public async Task<AiSupportResult> AnalyzeMessageAsync(string messageText)
        {
            // PROMPT MİMARİSİ
            string systemPrompt = @"
Sen GMK360 gayrimenkul sisteminin destek asistanısın. Gelen şu mesajı oku ve sadece şu 2 kategoriden birini JSON formatında dön:
1: 'AutoReply' (Şifre sıfırlama, sistemin nasıl kullanıldığı, aidatın nereden ödendiği gibi basit sorular. Bu durumda uygun bir cevap metni de üret.)
2: 'HumanEscalation' (Mahkeme, tahliye, karmaşık vergi itirazları, şikayetler. Bu durumda sadece 'Uzmana aktarıldı' mesajı dön.)

Beklenen JSON Formatı:
{
  ""Intent"": ""AutoReply"", // veya ""HumanEscalation""
  ""AiGeneratedResponse"": ""Cevabınız..."" 
}
";
            try
            {
                // TODO: Gerçek Gemini API Çağrısı Burada Yapılacak
                // var response = await _geminiClient.GenerateTextAsync(systemPrompt + "\nKullanıcı Mesajı: " + messageText);
                // var result = JsonSerializer.Deserialize<AiSupportResult>(response);

                // Mock Simülasyon
                bool isComplex = messageText.ToLower().Contains("mahkeme") || messageText.ToLower().Contains("şikayet");
                
                if (isComplex)
                {
                    return new AiSupportResult
                    {
                        Intent = "HumanEscalation",
                        AiGeneratedResponse = null
                    };
                }
                else
                {
                    return new AiSupportResult
                    {
                        Intent = "AutoReply",
                        AiGeneratedResponse = "Merhaba, aidat ödemelerinizi Dijital Evim > Cüzdanım sekmesinden güvenle yapabilirsiniz. Başka sorunuz olursa buradayım!"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini AI Analizinde Hata");
                // Hata durumunda güvenli liman: İnsana aktar (HumanEscalation)
                return new AiSupportResult { Intent = "HumanEscalation", AiGeneratedResponse = null };
            }
        }
    }
}
