using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using GMK360.Web.Services;

namespace GMK360.Web.Controllers
{
    public class KmkAskRequestDto
    {
        public string Question { get; set; } = "";
        public string? ActiveSlug { get; set; }
    }

    [Authorize]
    public class AiAssistantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly KmkAiAdvisorService _advisorService;

        public AiAssistantController(
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            KmkAiAdvisorService advisorService)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _advisorService = advisorService;
        }

        [AllowAnonymous]
        [HttpPost("/AiAssistant/Ask")]
        [EnableRateLimiting("GlobalLimiter")]
        public async Task<IActionResult> Ask([FromBody] KmkAskRequestDto? request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new { error = "Lütfen danışmak istediğiniz konuyu veya soruyu yazın." });
            }

            string cleanQ = request.Question.Trim();
            if (cleanQ.Length > 1000)
            {
                cleanQ = cleanQ.Substring(0, 1000);
            }

            bool isMember = User?.Identity?.IsAuthenticated == true;
            var result = await _advisorService.AskAsync(cleanQ, isMember, request.ActiveSlug);
            return Json(result);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromForm] string message, [FromForm] IFormFile? file)
        {
            try
            {
                string geminiApiKey = _config["GeminiApiKey"];
                if (string.IsNullOrEmpty(geminiApiKey))
                {
                    return Json(new { success = false, message = "Sistem yapılandırma hatası: AI API anahtarı eksik." });
                }

                var client = _httpClientFactory.CreateClient();
                var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={geminiApiKey}";

                var parts = new List<object>
                {
                    new { text = message ?? "Sana bir görsel gönderdim, lütfen incele." }
                };

                // Eğer dosya (görsel/fatura) yüklendiyse
                if (file != null && file.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    byte[] fileBytes = ms.ToArray();
                    string base64String = Convert.ToBase64String(fileBytes);
                    
                    parts.Add(new
                    {
                        inline_data = new
                        {
                            mime_type = file.ContentType,
                            data = base64String
                        }
                    });
                }

                string systemPrompt = "Sen 'GMK360' (Türkiye'nin yeni nesil dijital emlak, site yönetimi ve hizmet ekosistemi) platformunun resmi iç yapay zeka asistanı olan 'Asker'sin. Görevin: Sistem kullanıcılarına, emlakçılara ve yöneticilere yardımcı olmaktır. Gayrimenkul hukuku, kira gelir vergisi, giderlerin vergiden düşülmesi gibi konularda uzman tavsiyesi verirsin. Eğer sana bir fatura, fiş veya makbuz fotoğrafı gönderilirse, üzerindeki yazıları okuyup vergi uzmanı gibi yorumlar, 'Şu kadarı KDV, şu kadarı vergiden düşülebilir' gibi yönlendirmeler yaparsın. Her zaman çok nazik, net, Türkçe kurallarına uyan ve profesyonel bir dil kullanırsın.";

                var requestBody = new
                {
                    systemInstruction = new {
                        parts = new[] { new { text = systemPrompt } }
                    },
                    contents = new[]
                    {
                        new { parts = parts }
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(geminiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorStr = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = "AI Asker ile iletişim kurulamadı.", error = errorStr });
                }

                var jsonStr = await response.Content.ReadAsStringAsync();
                using (var doc = JsonDocument.Parse(jsonStr))
                {
                    var aiText = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                    
                    // Basit markdown to html (sadece kalin yazilari dondurelim simdilik)
                    var htmlText = aiText.Replace("\n", "<br/>").Replace("**", "<b>").Replace("<b>", "<b>").Replace("</b>", "</b>"); // basit düzeltme
                    
                    return Json(new { success = true, reply = htmlText });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Beklenmeyen bir hata oluştu: " + ex.Message });
            }
        }
    }
}
