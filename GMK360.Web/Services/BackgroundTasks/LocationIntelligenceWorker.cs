using GMK360.Core.Entities;
using GMK360.Core.Enums;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GMK360.Web.Services.BackgroundTasks
{
    public class LocationIntelligenceWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LocationIntelligenceWorker> _logger;
        private readonly HttpClient _httpClient;

        // DevamlÄ±lÄ±ÄŸÄ± engellemek iÃ§in taramanÄ±n bitip bitmediÄŸini tutan state
        private bool _isScanningComplete = false;

        private int _googleApiMonthlyCount = 0;
        private const int MAX_GOOGLE_API_COUNT = 39500;
        private bool _isGoogleApiCircuitBroken = false;
        private DateTime _lastResetDate = DateTime.UtcNow;

        public LocationIntelligenceWorker(IServiceProvider serviceProvider, ILogger<LocationIntelligenceWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "GMK360-IntelligenceWorker/1.0");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LocationIntelligenceWorker başlatıldı."); await Task.Yield();

            while (!stoppingToken.IsCancellationRequested && !_isScanningComplete)
            {
                try
                {
                    // AyÄ±n 1'inde sayacÄ± ve ÅŸalteri sÄ±fÄ±rla
                    if (DateTime.UtcNow.Month != _lastResetDate.Month)
                    {
                        _googleApiMonthlyCount = 0;
                        _isGoogleApiCircuitBroken = false;
                        _lastResetDate = DateTime.UtcNow;
                        _logger.LogInformation("Yeni aya girildi. Google API sayacÄ± ve ÅŸalteri sÄ±fÄ±rlandÄ±.");
                    }

                    await ProcessMissingNeighborhoods(stoppingToken);

                    // DÃ¶ngÃ¼ tamamlandÄ±ysa ve eksik kalmadÄ±ysa Worker'Ä± uykuya geÃ§iriyoruz (Ã¶rn. 24 saat)
                    if (_isScanningComplete)
                    {
                        _logger.LogInformation("TÃ¼m mahallelerin koordinat ve POI taramasÄ± tamamlandÄ±. Worker uyku moduna geÃ§iyor...");
                        try { await Task.Delay(TimeSpan.FromHours(24), stoppingToken); } catch { break; }
                        _isScanningComplete = false; // Bir sonraki gÃ¼n tekrar kontrol etmesi iÃ§in
                    }
                }
                catch (OperationCanceledException) { break; } catch (Exception ex) { try { _logger.LogError(ex, "Hata"); } catch { } try { await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); } catch { break; } }
            }
        }

        private async Task ProcessMissingNeighborhoods(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // KoordinatÄ± olmayan veya hiÃ§ POI kaydÄ± olmayan mahalleleri bul (Optimizasyon: batch halinde alalÄ±m)
            var neighborhoodsToProcess = await context.Neighborhoods
                .Include(n => n.District)
                .ThenInclude(d => d.City)
                .Where(n => !context.NeighborhoodPOIs.Any(p => p.NeighborhoodId == n.Id))
                .Take(10) // Her dÃ¶ngÃ¼de 10 tane iÅŸle (Rate limiting'e takÄ±lmamak iÃ§in)
                .ToListAsync(stoppingToken);

            if (!neighborhoodsToProcess.Any())
            {
                _isScanningComplete = true;
                return;
            }

            foreach (var neighborhood in neighborhoodsToProcess)
            {
                if (stoppingToken.IsCancellationRequested) break;

                // 1. KoordinatlarÄ± Bul
                var query = $"{neighborhood.Name}, {neighborhood.District.Name}, {neighborhood.District.City.Name}";
                var (lat, lon) = await GetCoordinatesHybridAsync(query, stoppingToken);

                if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lon))
                {
                    _logger.LogWarning($"Koordinat bulunamadÄ±: {query}");
                    continue; // Bir sonrakine geÃ§
                }

                // Neighborhood'a koordinatlarÄ± kaydet
                neighborhood.Latitude = lat;
                neighborhood.Longitude = lon;

                // 2. Etraftaki POI'leri Bul (2km = 2000m)
                var pois = await GetPOIsHybridAsync(neighborhood.Id, lat, lon, stoppingToken);

                context.NeighborhoodPOIs.AddRange(pois);

                _logger.LogInformation($"Mahalle POI'leri baÅŸarÄ±yla eklendi: {neighborhood.Name}");

                // API Ban yememek iÃ§in bekle (Nominatim Rate Limit)
                await Task.Delay(1500, stoppingToken); 
            }

            await context.SaveChangesAsync(stoppingToken);
        }

        private async Task<(string lat, string lon)> GetCoordinatesHybridAsync(string query, CancellationToken stoppingToken)
        {
            if (!_isGoogleApiCircuitBroken && _googleApiMonthlyCount < MAX_GOOGLE_API_COUNT)
            {
                try
                {
                    // GOOGLE MAPS API (Simulated or Real if Key exists)
                    _googleApiMonthlyCount++;
                    // var response = await _httpClient.GetAsync($"https://maps.googleapis.com/maps/api/geocode/json?address={query}&key=YOUR_API_KEY");
                    // if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests) throw new Exception("Quota Exceeded");
                    
                    // For safety, throw to simulate fail and trigger fallback to Nominatim immediately since we don't have a key
                    throw new Exception("Google API Key bulunamadÄ± veya Quota Exceeded simÃ¼lasyonu.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Google Geocoding baÅŸarÄ±sÄ±z oldu. Åalter indiriliyor. Hata: {ex.Message}");
                    _isGoogleApiCircuitBroken = true; // Circuit Breaker Trips!
                }
            }

            // FALLBACK: Nominatim (OpenStreetMap)
            try
            {
                await Task.Delay(1500, stoppingToken); // Rate limiting (KÄ±sÄ±k AteÅŸ Modu)
                var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=1";
                var response = await _httpClient.GetStringAsync(url, stoppingToken);
                var doc = JsonDocument.Parse(response);
                if (doc.RootElement.GetArrayLength() > 0)
                {
                    var firstResult = doc.RootElement[0];
                    return (firstResult.GetProperty("lat").GetString(), firstResult.GetProperty("lon").GetString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Nominatim Geocoding de baÅŸarÄ±sÄ±z oldu: {ex.Message}");
            }

            return (null, null);
        }

        private async Task<List<NeighborhoodPOI>> GetPOIsHybridAsync(int neighborhoodId, string lat, string lon, CancellationToken stoppingToken)
        {
            if (!_isGoogleApiCircuitBroken && _googleApiMonthlyCount < MAX_GOOGLE_API_COUNT)
            {
                try
                {
                    // GOOGLE PLACES API
                    _googleApiMonthlyCount++;
                    throw new Exception("Google Places API Key bulunamadÄ± simÃ¼lasyonu.");
                }
                catch
                {
                    _isGoogleApiCircuitBroken = true;
                }
            }

            // FALLBACK: Overpass API (OpenStreetMap)
            var pois = new List<NeighborhoodPOI>();
            try
            {
                await Task.Delay(2000, stoppingToken); // Rate Limiting for Overpass
                
                // Overpass QL Query for 2000m radius
                var overpassQuery = $@"[out:json];
(
  node[""amenity""=""school""](around:2000,{lat},{lon});
  node[""amenity""=""hospital""](around:2000,{lat},{lon});
  node[""amenity""=""police""](around:2000,{lat},{lon});
  node[""leisure""=""park""](around:2000,{lat},{lon});
  node[""shop""=""mall""](around:2000,{lat},{lon});
);
out center;";

                var content = new StringContent(overpassQuery);
                var response = await _httpClient.PostAsync("https://overpass-api.de/api/interpreter", content, stoppingToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync(stoppingToken);
                    var doc = JsonDocument.Parse(jsonStr);
                    var elements = doc.RootElement.GetProperty("elements");

                    foreach (var element in elements.EnumerateArray())
                    {
                        var tags = element.GetProperty("tags");
                        string name = tags.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "Bilinmeyen Yer";
                        
                        // Parse Category
                        POICategory category = POICategory.SocialAndPark;
                        if (tags.TryGetProperty("amenity", out var amenity))
                        {
                            var amStr = amenity.GetString();
                            if (amStr == "school") category = POICategory.Education;
                            else if (amStr == "hospital") category = POICategory.Health;
                            else if (amStr == "police") category = POICategory.Security;
                        }
                        else if (tags.TryGetProperty("shop", out var shop) && shop.GetString() == "mall")
                        {
                            category = POICategory.Shopping;
                        }

                        // Calculate rough distance (Euclidean approx in meters for simplicity, though actual distance needs Haversine)
                        var pLat = element.GetProperty("lat").GetDouble();
                        var pLon = element.GetProperty("lon").GetDouble();
                        var dist = CalculateDistance(double.Parse(lat, System.Globalization.CultureInfo.InvariantCulture), double.Parse(lon, System.Globalization.CultureInfo.InvariantCulture), pLat, pLon);

                        pois.Add(new NeighborhoodPOI
                        {
                            NeighborhoodId = neighborhoodId,
                            PoiName = name,
                            PoiCategory = category,
                            DistanceInMeters = dist,
                            Latitude = pLat.ToString(System.Globalization.CultureInfo.InvariantCulture),
                            Longitude = pLon.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Overpass API hatasÄ±: {ex.Message}");
            }

            return pois;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371e3; // metres
            var phi1 = lat1 * Math.PI / 180;
            var phi2 = lat2 * Math.PI / 180;
            var deltaPhi = (lat2 - lat1) * Math.PI / 180;
            var deltaLambda = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                    Math.Cos(phi1) * Math.Cos(phi2) *
                    Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return Math.Round(r * c);
        }
    }
}



