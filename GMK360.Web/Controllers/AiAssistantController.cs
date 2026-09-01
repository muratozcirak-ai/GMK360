using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class AiAssistantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public AiAssistantController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
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
                    return Json(new { success = false, message = "Sistem yapýlandýrma hatasý: AI API anahtarý eksik." });
                }

                var client = _httpClientFactory.CreateClient();
                var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={geminiApiKey}";

                var parts = new List<object>
                {
                    new { text = message ?? "Sana bir görsel gönderdim, lütfen incele." }
                };

                // Eðer dosya (görsel/fatura) yüklendiyse
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

                string systemPrompt = "Sen 'GMK360' (Türkiye'nin yeni nesil dijital emlak, site yönetimi ve hizmet ekosistemi) platformunun resmi iç yapay zeka asistaný olan 'Asker'sin. Görevin: Sistem kullanýcýlarýna, emlakçýlara ve yöneticilere yardýmcý olmaktýr. Gayrimenkul hukuku, kira gelir vergisi, giderlerin vergiden düþülmesi gibi konularda uzman tavsiyesi verirsin. Eðer sana bir fatura, fiþ veya makbuz fotoðrafý gönderilirse, üzerindeki yazýlarý okuyup vergi uzmaný gibi yorumlar, 'Þu kadarý KDV, þu kadarý vergiden düþülebilir' gibi yönlendirmeler yaparsýn. Her zaman çok nazik, net, Türkçe kurallarýna uyan ve profesyonel bir dil kullanýrsýn.";

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
                    return Json(new { success = false, message = "AI Asker ile iletiþim kurulamadý.", error = errorStr });
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
                return Json(new { success = false, message = "Beklenmeyen bir hata oluþtu: " + ex.Message });
            }
        }
    }
}
