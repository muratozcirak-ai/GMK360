using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace GMK360.Web.Services
{
    public class ValuationService : IValuationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<ValuationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ValuationService(ApplicationDbContext context, IConfiguration config, ILogger<ValuationService> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _config = config;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task RequestValuationAsync(string il, string ilce, string mahalle, string mulkTipi, string odaSayisi, string email)
        {
            try
            {
                var existing = await _context.BolgeEkspertizHafizalari.FirstOrDefaultAsync(x => 
                    x.Il == il && 
                    x.Ilce == ilce && 
                    x.Mahalle == mahalle && 
                    x.MulkTipi == mulkTipi && 
                    x.OdaSayisi == odaSayisi && 
                    x.SorgulamaTarihi >= DateTime.Now.AddDays(-30));

                if (existing != null)
                {
                    _logger.LogInformation($"Valuation retrieved from cache for {il} {ilce} {mahalle}. Cost: 0.");
                    return;
                }

                // 1. Get Coordinates using Google Geocoding API
                string mapsApiKey = _config["GoogleMapsApiKey"];
                string geminiApiKey = _config["GeminiApiKey"];
                
                double lat = 0, lng = 0;
                var client = _httpClientFactory.CreateClient();
                
                string address = $"{mahalle} {ilce} {il} Turkey";
                var geoUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={mapsApiKey}";
                
                var geoResponse = await client.GetStringAsync(geoUrl);
                using (var doc = JsonDocument.Parse(geoResponse))
                {
                    var root = doc.RootElement;
                    if (root.GetProperty("status").GetString() == "OK")
                    {
                        var location = root.GetProperty("results")[0].GetProperty("geometry").GetProperty("location");
                        lat = location.GetProperty("lat").GetDouble();
                        lng = location.GetProperty("lng").GetDouble();
                    }
                }

                // 2. Get Nearby Places
                var placesList = new List<object>();
                string[] types = { "hospital", "school", "pharmacy", "police", "park", "transit_station" };
                
                if (lat != 0 && lng != 0)
                {
                    foreach (var type in types)
                    {
                        var placesUrl = $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={lat},{lng}&radius=2000&type={type}&key={mapsApiKey}";
                        var placesResp = await client.GetStringAsync(placesUrl);
                        
                        using (var pDoc = JsonDocument.Parse(placesResp))
                        {
                            var pRoot = pDoc.RootElement;
                            if (pRoot.GetProperty("status").GetString() == "OK")
                            {
                                var results = pRoot.GetProperty("results");
                                int count = 0;
                                foreach (var item in results.EnumerateArray())
                                {
                                    if (count >= 2) break; // Sadece en yakin 2 taneyi al
                                    string name = item.GetProperty("name").GetString();
                                    placesList.Add(new { Type = type, Name = name });
                                    count++;
                                }
                            }
                        }
                    }
                }
                
                string donatilarJson = JsonSerializer.Serialize(placesList);

                // 3. Get Valuation from Gemini AI
                decimal ortalamaKira = 0;
                decimal ortalamaSatis = 0;
                int amortisman = 0;
                
                string prompt = $"Sen kıdemli bir gayrimenkul değerleme uzmanısın. Lütfen {il} ili, {ilce} ilçesi, {mahalle} mahallesindeki {odaSayisi} oda özelliklerine sahip standart bir {mulkTipi} için internetteki güncel emlak verilerini analiz ederek şu üç bilgiyi JSON formatında dön: 1. Ortalama aylık kira bedeli (Sadece rakamsal, örn: 15000), 2. Ortalama satış değeri (Sadece rakamsal, örn: 3500000), 3. Amortisman süresi yıl (Sadece rakam, örn: 19). Return purely JSON object with properties 'Kira', 'Satis', 'Amortisman'.";
                
                var geminiRequest = new
                {
                    contents = new[] { new { parts = new[] { new { text = prompt } } } },
                    generationConfig = new { responseMimeType = "application/json" }
                };
                
                var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={geminiApiKey}";
                var content = new StringContent(JsonSerializer.Serialize(geminiRequest), System.Text.Encoding.UTF8, "application/json");
                
                var aiResponse = await client.PostAsync(geminiUrl, content);
                if (aiResponse.IsSuccessStatusCode)
                {
                    var aiJsonStr = await aiResponse.Content.ReadAsStringAsync();
                    using (var aiDoc = JsonDocument.Parse(aiJsonStr))
                    {
                        var text = aiDoc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                        
                        using (var resultDoc = JsonDocument.Parse(text))
                        {
                            ortalamaKira = resultDoc.RootElement.GetProperty("Kira").GetDecimal();
                            ortalamaSatis = resultDoc.RootElement.GetProperty("Satis").GetDecimal();
                            amortisman = resultDoc.RootElement.GetProperty("Amortisman").GetInt32();
                        }
                    }
                }

                // 4. Save to Database
                var newRecord = new BolgeEkspertizHafizasi
                {
                    Il = il,
                    Ilce = ilce,
                    Mahalle = mahalle,
                    MulkTipi = mulkTipi,
                    OdaSayisi = odaSayisi,
                    OrtalamaKira = ortalamaKira > 0 ? ortalamaKira : 15000,
                    OrtalamaSatisDegeri = ortalamaSatis > 0 ? ortalamaSatis : 3500000,
                    AmortismanSuresiYil = amortisman > 0 ? amortisman : 20,
                    YakinDonatilarJSON = donatilarJson,
                    SorgulamaTarihi = DateTime.Now
                };

                _context.BolgeEkspertizHafizalari.Add(newRecord);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"New valuation generated and cached for {il} {ilce} {mahalle}.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing valuation request");
            }
        }
    }
}
