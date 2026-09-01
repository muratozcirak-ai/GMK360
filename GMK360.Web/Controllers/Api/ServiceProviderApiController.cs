using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceProviderApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceProviderApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] int districtId, [FromQuery] int categoryId)
        {
            var query = _context.ServiceProviders
                .Include(sp => sp.User)
                .Where(sp => sp.Services.Any(s => s.ServiceCategoryId == categoryId));

            if (districtId > 0)
            {
                query = query.Where(sp => sp.IsGlobal || sp.Areas.Any(a => a.DistrictId == districtId));
            }

            var result = await query.Select(sp => new
            {
                sp.Id,
                sp.BusinessName,
                sp.TotalRatings,
                sp.AverageRating,
                ContactPhone = sp.User.PhoneNumber
            }).ToListAsync();

            return Ok(result);
        }

        [HttpPost("Rate")]
        [Authorize]
        public async Task<IActionResult> RateServiceProvider([FromBody] RateProviderRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var provider = await _context.ServiceProviders.FindAsync(request.ServiceProviderId);
            if (provider == null) return NotFound("Esnaf bulunamadı.");

            // Müşteri bu esnafla daha önce iletişime geçti mi? (Numarayı Gör'e tıkladı mı?)
            var contactLog = await _context.ContactLogs
                .Where(cl => cl.UserId == userId && cl.ServiceProviderId == request.ServiceProviderId)
                .OrderByDescending(cl => cl.ContactDate)
                .FirstOrDefaultAsync();

            if (contactLog == null)
            {
                return BadRequest("Sadece iletişime geçtiğiniz esnafları puanlayabilirsiniz.");
            }

            // Spam Kontrolü: Aynı ay içinde sadece 1 kez puan verebilir
            var lastRating = await _context.ServiceProviderRatings
                .Where(r => r.UserId == userId && r.ServiceProviderId == request.ServiceProviderId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastRating != null && lastRating.CreatedAt.AddDays(30) > DateTime.UtcNow)
            {
                return BadRequest("Bu esnafa son 30 gün içinde zaten puan verdiniz.");
            }

            var rating = new ServiceProviderRating
            {
                ServiceProviderId = request.ServiceProviderId,
                UserId = userId,
                Rating = request.Rating,
                Comment = request.Comment,
                IsApproved = false // Admin onayı
            };

            _context.ServiceProviderRatings.Add(rating);
            
            // Ortalama puanı güncelle
            provider.TotalRatings += 1;
            // Yeni ortalama = ((Eski Ortalama * Eski Toplam) + Yeni Puan) / Yeni Toplam
            double currentTotalScore = provider.AverageRating * (provider.TotalRatings - 1);
            provider.AverageRating = (currentTotalScore + request.Rating) / provider.TotalRatings;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Değerlendirmeniz başarıyla alındı ve onay sürecine gönderildi." });
        }
    }

    public class RateProviderRequest
    {
        public int ServiceProviderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
