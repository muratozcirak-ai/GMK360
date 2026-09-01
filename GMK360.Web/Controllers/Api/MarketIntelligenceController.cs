using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using GMK360.Data.Contexts;
using GMK360.Core.DTOs;
using GMK360.Core.Entities;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketIntelligenceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private const string SECRET_API_KEY = "Cati360_Ozel_Gizli_Anahtar_2026"; // Sadece n8n'in bildiği şifre

        public MarketIntelligenceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("ReceiveData")]
        public async Task<IActionResult> ReceiveData([FromHeader(Name = "x-api-key")] string apiKey, [FromBody] MarketDataIncomingDto data)
        {
            // Güvenlik Kontrolü: İstek gerçekten bizim n8n sunucumuzdan mı geliyor?
            if (apiKey != SECRET_API_KEY)
            {
                return Unauthorized("Erişim reddedildi.");
            }

            // Mükerrer Kayıt Kontrolü: Bu ilan daha önce çekilmiş mi?
            bool exists = await _context.MarketAnalytics.AnyAsync(m => m.OriginalSourceId == data.OriginalSourceId);
            if (exists)
            {
                return Ok("Veri zaten mevcut, işlem atlandı.");
            }

            // Yeni veriyi oluştur ve m2 fiyatını hesapla
            var newRecord = new MarketAnalytics
            {
                City = data.City,
                District = data.District,
                Neighborhood = data.Neighborhood,
                PropertyType = data.PropertyType,
                Price = data.Price,
                SquareMeters = data.SquareMeters,
                PricePerSquareMeter = (decimal)(data.Price / (decimal)data.SquareMeters),
                RecordDate = DateTime.UtcNow,
                OriginalSourceId = data.OriginalSourceId,
                SourcePlatform = data.SourcePlatform, // Kaynak adı eklendi
                SourceUrl = data.SourceUrl,           // İlan linki eklendi
                IsActive = true
            };

            _context.MarketAnalytics.Add(newRecord);
            await _context.SaveChangesAsync();

            return Ok("İstihbarat verisi başarıyla arşive eklendi.");
        }
    }
}
