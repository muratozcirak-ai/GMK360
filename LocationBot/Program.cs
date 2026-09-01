using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;

namespace LocationBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== GMK360 Location Intelligence Bot ===");
            Console.WriteLine("Starting bot to find coordinates and POIs...");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GMK360DB;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False");

            using var db = new ApplicationDbContext(optionsBuilder.Options);
            using var httpClient = new HttpClient();
            
            // Nominatim requires a User-Agent
            httpClient.DefaultRequestHeaders.Add("User-Agent", "GMK360LocationBot/1.0");

            // Örnek olarak SADECE İstanbul (CityId=34), Kadıköy (DistrictId=...) ve koordinatı boş olanlardan 5 tane alalım test için.
            // Gerçekte bu limit kalkar ve sürekli arka planda çalışır.
            var streetsToProcess = await db.Streets
                .Include(s => s.Neighborhood)
                .ThenInclude(n => n.District)
                .ThenInclude(d => d.City)
                .Where(s => s.Latitude == null && s.Neighborhood.District.City.Name == "İstanbul")
                .Take(5) // Sadece 5 sokak test için
                .ToListAsync();

            if (!streetsToProcess.Any())
            {
                Console.WriteLine("No streets without coordinates found in Istanbul.");
                return;
            }

            foreach (var street in streetsToProcess)
            {
                Console.WriteLine($"\nProcessing: {street.Neighborhood.District.City.Name}, {street.Neighborhood.District.Name}, {street.Neighborhood.Name}, {street.Name}");

                // 1. Nominatim API ile Koordinat Bul
                string query = $"{street.Name}, {street.Neighborhood.Name}, {street.Neighborhood.District.Name}, {street.Neighborhood.District.City.Name}, Turkey";
                string geocodeUrl = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=1";

                try
                {
                    var response = await httpClient.GetStringAsync(geocodeUrl);
                    using var doc = JsonDocument.Parse(response);
                    
                    if (doc.RootElement.GetArrayLength() > 0)
                    {
                        var firstResult = doc.RootElement[0];
                        string lat = firstResult.GetProperty("lat").GetString();
                        string lon = firstResult.GetProperty("lon").GetString();

                        street.Latitude = lat;
                        street.Longitude = lon;
                        Console.WriteLine($"[+] Coordinates found: Lat {lat}, Lon {lon}");

                        // 2. Overpass API ile Yakındaki Yerleri (POI) Bul (Örn: 1000 metre çapında Okullar)
                        // This is a basic Overpass QL query to find amenities around a coordinate
                        string overpassQuery = $@"[out:json][timeout:25];
                        (
                          node[""amenity""=""school""](around:1000,{lat},{lon});
                          node[""amenity""=""hospital""](around:1000,{lat},{lon});
                          node[""leisure""=""park""](around:1000,{lat},{lon});
                        );
                        out body;
                        >;
                        out skel qt;";

                        var overpassResponse = await httpClient.PostAsync("https://overpass-api.de/api/interpreter", new StringContent(overpassQuery));
                        if (overpassResponse.IsSuccessStatusCode)
                        {
                            var overpassJson = await overpassResponse.Content.ReadAsStringAsync();
                            using var overpassDoc = JsonDocument.Parse(overpassJson);
                            var elements = overpassDoc.RootElement.GetProperty("elements");

                            int poiCount = 0;
                            foreach (var element in elements.EnumerateArray())
                            {
                                if (element.TryGetProperty("tags", out var tags) && tags.TryGetProperty("name", out var nameProp))
                                {
                                    string poiName = nameProp.GetString();
                                    string type = "Bilinmiyor";
                                    
                                    if (tags.TryGetProperty("amenity", out var amenity)) type = amenity.GetString();
                                    else if (tags.TryGetProperty("leisure", out var leisure)) type = leisure.GetString();

                                    // Simple haversine distance or just storing it.
                                    var poiLat = element.GetProperty("lat").GetDouble();
                                    var poiLon = element.GetProperty("lon").GetDouble();

                                    db.NearbyPlaces.Add(new NearbyPlace
                                    {
                                        StreetId = street.Id,
                                        Name = poiName,
                                        Type = type,
                                        Latitude = poiLat.ToString(),
                                        Longitude = poiLon.ToString(),
                                        DistanceInMeters = 500, // Varsayımsal/Hesaplanabilir
                                        CreatedAt = DateTime.UtcNow
                                    });
                                    poiCount++;
                                }
                            }
                            Console.WriteLine($"[+] Found {poiCount} nearby POIs (Schools, Hospitals, Parks).");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[-] Coordinates not found.");
                        // To prevent retrying failing streets constantly, set dummy or flag
                        street.Latitude = "NOT_FOUND";
                        street.Longitude = "NOT_FOUND";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[!] Error: {ex.Message}");
                }

                // Nominatim allows max 1 request per second. Overpass is also strict.
                await Task.Delay(2000); 
            }

            await db.SaveChangesAsync();
            Console.WriteLine("\nDatabase updated successfully!");
        }
    }
}
