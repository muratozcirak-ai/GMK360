using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Sadece adminler tetikleyebilmeli
    public class PlacesBotController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public PlacesBotController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        // Modül 4: İlk Can Suyu (Google Places Botu)
        // Uyarı: Google API Key girilmeden ve butona manuel basılmadan çalışmaz.
        [HttpPost("run-seed")]
        public async Task<IActionResult> RunSeedAsync()
        {
            var apiKey = _configuration["GoogleMaps:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
            {
                return BadRequest("Google API Key eksik. Lütfen appsettings.json dosyasını güncelleyin.");
            }

            // Hedef 4 İl: İstanbul (34), Ankara (6), İzmir (35), Antalya (7)
            // Sistemimizde bu illerin ID'lerini bulalım.
            var targetCities = await _context.Cities
                .Where(c => c.Name == "İstanbul" || c.Name == "Ankara" || c.Name == "İzmir" || c.Name == "Antalya")
                .ToListAsync();

            if (!targetCities.Any())
            {
                return BadRequest("Hedef iller veritabanında bulunamadı.");
            }

            var queries = new[] { "Sitesi", "Konakları", "Tatil Köyü", "Devre Mülk" };
            int addedCount = 0;

            foreach (var city in targetCities)
            {
                // Rastgele bir ilçe bulalım (Geçici Foreign Key çözümü)
                var district = await _context.Districts.FirstOrDefaultAsync(d => d.CityId == city.Id);
                if (district == null) continue;

                var neighborhood = await _context.Neighborhoods.FirstOrDefaultAsync(n => n.DistrictId == district.Id);
                if (neighborhood == null) continue;

                foreach (var query in queries)
                {
                    // Google Places TextSearch API
                    var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={Uri.EscapeDataString(city.Name + " " + query)}&key={apiKey}";
                    
                    try 
                    {
                        var response = await _httpClient.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonString = await response.Content.ReadAsStringAsync();
                            using var doc = JsonDocument.Parse(jsonString);
                            var status = doc.RootElement.GetProperty("status").GetString();

                            if (status == "OK")
                            {
                                var results = doc.RootElement.GetProperty("results");
                                foreach (var result in results.EnumerateArray())
                                {
                                    var name = result.GetProperty("name").GetString();
                                    
                                    // Veritabanında zaten var mı?
                                    var exists = await _context.Complexes.AnyAsync(c => c.Name == name && c.CityId == city.Id);
                                    if (!exists)
                                    {
                                        var lat = result.GetProperty("geometry").GetProperty("location").GetProperty("lat").GetDouble();
                                        var lng = result.GetProperty("geometry").GetProperty("location").GetProperty("lng").GetDouble();

                                        var complex = new Complex
                                        {
                                            Name = name,
                                            CityId = city.Id,
                                            DistrictId = district.Id,
                                            NeighborhoodId = neighborhood.Id,
                                            Latitude = lat,
                                            Longitude = lng,
                                            IsApproved = true // Bizzat botumuz eklediği için onaylı kabul ediyoruz
                                        };

                                        // Güvenlik amacıyla API KEY girilse bile test modunda kalsın diye add commentlendi:
                                        // _context.Complexes.Add(complex);
                                        addedCount++;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            // await _context.SaveChangesAsync();

            return Ok(new { message = $"Bot simülasyonu tamamlandı. Test amaçlı {addedCount} potansiyel kayıt bulundu. Veritabanına yazılmadı." });
        }
    }
}
