using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppraisalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppraisalController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Modül 5.3: Kurumsal Truva Atı (Ekspertiz Modülü)
        // Kullanıcıya ücretsiz ev değerleme tahmini sunar
        [HttpGet("Estimate")]
        public async Task<IActionResult> EstimateValue(int neighborhoodId, int subTypeId, int m2)
        {
            if (neighborhoodId <= 0 || subTypeId <= 0 || m2 <= 0)
            {
                return BadRequest("Geçersiz parametreler.");
            }

            // O mahalledeki ve o alt türdeki (Örn: Daire) tüm aktif ilanları çek
            // ve PriceHistory ile fiyat geçmişini de dahil et
            var properties = await _context.Properties
                .Include(p => p.Complex)
                .Where(p => p.Complex.NeighborhoodId == neighborhoodId && p.SubTypeId == subTypeId && p.State == GMK360.Core.Entities.ListingState.Active)
                .Select(p => new
                {
                    p.Id,
                    p.Price,
                    p.NetArea // Net m2 farz ediyoruz ki entity'de var. Değilse eklenecek.
                })
                .ToListAsync();

            if (properties.Count < 3)
            {
                return Ok(new { success = false, message = "Bu bölgede yeterli veri bulunamadığı için ekspertiz yapılamıyor. Veri havuzumuz büyüdükçe tekrar deneyin." });
            }

            // Sadece NetArea > 0 olanları alalım
            var validProperties = properties.Where(p => p.NetArea > 0).ToList();
            if (validProperties.Count < 3)
            {
                return Ok(new { success = false, message = "Yeterli metrekare verisi bulunamadı." });
            }

            // m2 Fiyatlarını hesapla
            var m2Prices = validProperties.Select(p => p.Price / (decimal)p.NetArea).OrderBy(p => p).ToList();

            // %10 Outlier Tıraşlama (En pahalı ve en ucuz kısımları at)
            int skipCount = (int)Math.Ceiling(m2Prices.Count * 0.1);
            
            var filteredM2Prices = m2Prices.Skip(skipCount).Take(m2Prices.Count - (skipCount * 2)).ToList();

            if (!filteredM2Prices.Any())
            {
                // Çok az veri varsa tıraşlama yapma
                filteredM2Prices = m2Prices;
            }

            // Ortalama m2 Fiyatı
            decimal avgM2Price = filteredM2Prices.Average();

            // Tahmini Değer (Bant aralığı sunalım: %5 aşağısı ve %5 yukarısı)
            decimal estimatedValue = avgM2Price * m2;
            decimal minValue = estimatedValue * 0.95m;
            decimal maxValue = estimatedValue * 1.05m;

            return Ok(new
            {
                success = true,
                avgM2Price = Math.Round(avgM2Price, 2),
                estimatedMin = Math.Round(minValue, 2),
                estimatedMax = Math.Round(maxValue, 2),
                dataPointsUsed = filteredM2Prices.Count,
                message = "Değerleme başarıyla hesaplandı."
            });
        }
    }
}
