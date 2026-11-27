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
            if (string.IsNullOrEmpty(_apiKey))
            {
                return GenerateFallbackNeighborhoodReport(districtName, neighborhoodName);
            }

            string prompt = $@"
Sen Türkiye'nin en tecrübeli gayrimenkul yatırım ve kentsel dönüşüm bölge analiz uzmanısın.
Kullanıcı '{districtName} ilçesi, {neighborhoodName} mahallesi' için bir kentsel dönüşüm ve inşaat fizibilitesi bölge analizi istiyor.
Lütfen bu mahalle hakkında şu 5 başlığı içeren, müteahhit ve proje geliştiricisine yönelik profesyonel bir Markdown raporu hazırla:
1. Bölge Kimliği & Sosyo-Demografik Yapı (Yapı stoku, yerleşim yoğunluğu, halkın kentsel dönüşüme bakışı)
2. Ulaşım Ağı & Lokasyon Erişilebilirliği (Merkeze yakınlık, ana arterler, raylı sistem ve toplu ulaşım)
3. Konut Talebi & Gayrimenkul Dinamikleri (En çok talep gören daire tipleri, kiralama potansiyeli, değer artış beklentisi)
4. Bölge Fiyat Rayiçleri & Kira Endeksi (Tahmini sıfır ve eski bina satılık m² birim fiyat bandı, 2+1 ve 3+1 güncel ortalama kiralık daire fiyatları, kentsel dönüşüm kira yardımı kıyaslaması)
5. Müteahhit & Teklif Fizibilite Tavsiyesi (Kat karşılığı teklif oluştururken dikkat edilmesi gereken paylaşım dengesi, hak sahipleri beklentisi ve mimari standartlar)

Not: Kesinlikle JSON dönme, doğrudan Markdown formatında şık ve okunaklı bir metin dön.
";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { temperature = 0.4 }
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
                        var text = parts[0].GetProperty("text").GetString() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(text)) return text;
                    }
                }
                
                return GenerateFallbackNeighborhoodReport(districtName, neighborhoodName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini Neighborhood Analysis Error");
                return GenerateFallbackNeighborhoodReport(districtName, neighborhoodName);
            }
        }

        private string GenerateFallbackNeighborhoodReport(string districtName, string neighborhoodName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"### 📍 {districtName} / {neighborhoodName} — Kentsel Dönüşüm & Bölge Analizi");
            sb.AppendLine();
            sb.AppendLine("#### 1. Bölge Kimliği & Sosyo-Demografik Yapı");
            sb.AppendLine($"{neighborhoodName}, {districtName} ilçesinin konut yerleşimi oturmuş, sosyal dokusu güçlü ve aile demografisinin yoğun olduğu kıymetli bölgelerindendir. Bölgedeki mevcut yapı stoku ağırlıklı olarak 1999 öncesi yapılmış 4-6 katlı binalardan oluşmakta olup, deprem güvenliği ve kentsel dönüşüm farkındalığı mülk sahipleri nezdinde en üst düzeydedir.");
            sb.AppendLine();
            sb.AppendLine("#### 2. Ulaşım Ağı & Lokasyon Erişilebilirliği");
            sb.AppendLine($"{districtName} ana arterlerine, metro ve toplu taşıma hatlarına olan doğrudan bağlantıları sayesinde bölge yüksek erişilebilirliğe sahiptir. E-5 ve TEM akslarına olan yakınlık, hem Anadolu hem Avrupa yakası istihdam merkezlerine ulaşımı kolaylaştırmakta ve konut talebini daima diri tutmaktadır.");
            sb.AppendLine();
            sb.AppendLine("#### 3. Konut Talebi & Gayrimenkul Dinamikleri");
            sb.AppendLine($"Bölgede kentsel dönüşümle inşa edilecek yeni ve deprem yönetmeliğine tam uyumlu projelerde en yüksek talep 2+1 ve 3+1 aile tipi dairelerdedir. Kapalı otopark, asansör, balkon ve enerji verimliliği yüksek yapı standartları, sıfır binalara eski yapı stokuna kıyasla belirgin bir satış ve kiralama prim avantajı sağlamaktadır.");
            sb.AppendLine();
            sb.AppendLine("#### 4. Bölge Fiyat Rayiçleri & Kira Endeksi");
            sb.AppendLine($"* **Satılık Konut m² Rayici:** {districtName} / {neighborhoodName} genelinde sıfır ve deprem yönetmeliğine uygun yeni binalarda satılık m² birim fiyatları konuma, caddeye yakınlığa ve kata göre yüksek prim yapmaktadır. Dönüşüm gören sıfır binalar eski yapı stokuna kıyasla %50-70 daha yüksek piyasa değerine ulaşmaktadır.");
            sb.AppendLine($"* **Kiralık Konut Piyasası:** Bölgede aile tipi 2+1 ve 3+1 konutlara kesintisiz kiralama talebi bulunmaktadır. Kentsel dönüşüm süresince hak sahiplerine sağlanacak kira destekleri ve taşınma maliyetleri bölge rayiciyle tam uyumlu planlanmalıdır.");
            sb.AppendLine();
            sb.AppendLine("#### 5. Müteahhit & Teklif Fizibilite Tavsiyesi");
            sb.AppendLine($"* **Malik Uzlaşması:** {neighborhoodName} sakinleriyle yapılacak görüşmelerde mevcut net kullanım haklarının şeffaf sunulması ve makul şerefiye farklarının baştan belirlenmesi sözleşme sürecini hızlandıracaktır.");
            sb.AppendLine($"* **Müteahhit Payı Satış Stratejisi:** Projede firmaya kalacak bağımsız bölümlerin fonksiyonel mimariyle planlanması ve ön satış avantajı sunulması nakit akışını güvenceye alacaktır.");
            sb.AppendLine($"* **İmar & Resmi Süreç:** Resmi teklif imzalanmadan önce ilçe belediyesinden güncel imar çapı (KAKS/TAKS) ve zemin etüdü doğrulanarak nihai kütle yerleşimi kesinleştirilmelidir.");
            return sb.ToString();
        }

        public async Task<string> AskKmkAdvisorAsync(string question, string knowledgeContext, bool isMember, string? activeChapterTitle = null)
        {
            string roleNote = isMember
                ? "Kullanıcı GMK360 üye site/apartman yöneticisi veya sakinidir; örnek karar ve ihtarname taslağı da üretebilirsin."
                : "Kullanıcı halka açık ziyaretçidir; genel KMK kurallarını açıkla ve gerekirse GMK360 Yönetici El Kitabına yönlendir.";

            string activeNote = string.IsNullOrWhiteSpace(activeChapterTitle)
                ? ""
                : $"\nKullanıcının şu an incelediği bölüm: {activeChapterTitle}";

            string systemPrompt = $@"
Sen 634 sayılı Kat Mülkiyeti Kanunu (KMK) ve Türk Borçlar Kanunu (TBK) konusunda uzman GMK360 Dijital Mevzuat Danışmanısın.
{roleNote}{activeNote}

KURALLAR:
1. Yanıtını öncelikle aşağıda verilen GMK360 Merkezi Mevzuat Kütüphanesi metinlerine dayandır; ilgili KMK maddesini açıkça belirt.
2. KVKK gereği borçlu kişilerin isimlerinin veya dairelerinin başkalarına ifşa edilmemesi gerektiğini hatırla.
3. YALNIZCA aşağıdaki şemada geçerli JSON dön (başka metin ekleme):
{{
  ""answer"": ""Net, pratik Türkçe açıklama (3-6 cümle)"",
  ""draftText"": ""Kullanıcı karar metni veya ihtar taslağı istediyse kopyalanabilir taslak metin, istemediyse boş string"",
  ""recommendedSlug"": ""Verilen bağlamdaki en uygun kaynağın SLUG değeri"",
  ""recommendedTitle"": ""Verilen bağlamdaki en uygun kaynağın BAŞLIK değeri""
}}";

            string userPrompt = $"MERKEZİ KMK KÜTÜPHANESİ BAĞLAMI:\n{knowledgeContext}\n\nKULLANICI SORUSU:\n{question}";
            return await CallGeminiApiAsync(systemPrompt, userPrompt);
        }
    }
}

