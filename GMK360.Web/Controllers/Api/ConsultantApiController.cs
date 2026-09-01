using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultantApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConsultantApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("Rate")]
        [Authorize]
        public async Task<IActionResult> SubmitRating([FromBody] RatingRequestDto request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Spam Check: 1 ayda sadece 1 defa puan verebilir.
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentRating = await _context.ConsultantRatings
                .Where(r => r.UserId == user.Id && r.ConsultantId == request.ConsultantId && r.CreatedAt >= thirtyDaysAgo)
                .FirstOrDefaultAsync();

            if (recentRating != null)
            {
                return BadRequest("Bu danışmana son 30 gün içinde zaten puan verdiniz.");
            }

            // Müşteri bu danışmanla son 30 günde iletişime geçmiş mi? (Opsiyonel ama mantıklı bir spam koruması daha)
            var hasContact = await _context.ContactLogs
                .AnyAsync(c => c.UserId == user.Id && c.ConsultantId == request.ConsultantId && c.ContactDate >= thirtyDaysAgo);

            if (!hasContact)
            {
                return BadRequest("Sadece son 30 gün içinde iletişime geçtiğiniz danışmanları puanlayabilirsiniz.");
            }

            var rating = new ConsultantRating
            {
                ConsultantId = request.ConsultantId,
                UserId = user.Id,
                CommunicationScore = request.CommunicationScore,
                KnowledgeScore = request.KnowledgeScore,
                RecommendationScore = request.RecommendationScore,
                Comment = request.Comment,
                IsApproved = false, // Admin onayına düşer
                CreatedAt = DateTime.UtcNow
            };

            _context.ConsultantRatings.Add(rating);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Değerlendirmeniz başarıyla alındı ve onay sürecine gönderildi." });
        }

        [HttpGet("List")]
        public async Task<IActionResult> GetConsultants([FromQuery] int? cityId, [FromQuery] int? districtId, [FromQuery] string regionName)
        {
            var query = _userManager.Users.Where(u => u.UserType == GMK360.Core.Entities.Identity.UserType.Consultant);

            // TODO: İl, İlçe veya Yakaya göre filtreleme (Danışmanların uzmanlık alanları vs. eklendiğinde genişletilebilir)
            // Şimdilik sadece aktif ilanlarının bulunduğu bölgelere göre filtreleyebiliriz.

            var consultants = await query.ToListAsync();

            var result = consultants.Select(c => {
                var ratings = _context.ConsultantRatings.Where(r => r.ConsultantId == c.Id && r.IsApproved).ToList();
                var count = ratings.Count;

                // Zero-division check
                double commAvg = count > 0 ? ratings.Average(r => r.CommunicationScore) : 0;
                double knowAvg = count > 0 ? ratings.Average(r => r.KnowledgeScore) : 0;
                double recAvg = count > 0 ? ratings.Average(r => r.RecommendationScore) : 0;
                
                double overallAvg = count > 0 ? (commAvg + knowAvg + recAvg) / 3.0 : 0;

                return new {
                    c.Id,
                    c.FirstName,
                    c.LastName,
                    FullName = $"{c.FirstName} {c.LastName}",
                    c.ProfileImageUrl,
                    TotalRatings = count,
                    CommunicationAverage = commAvg,
                    KnowledgeAverage = knowAvg,
                    RecommendationAverage = recAvg,
                    OverallAverage = overallAvg
                };
            }).OrderByDescending(c => c.OverallAverage).ThenByDescending(c => c.TotalRatings).ToList();

            return Ok(result);
        }
    }

    public class RatingRequestDto
    {
        public string ConsultantId { get; set; }
        public int CommunicationScore { get; set; }
        public int KnowledgeScore { get; set; }
        public int RecommendationScore { get; set; }
        public string Comment { get; set; }
    }
}
