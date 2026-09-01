using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [IgnoreAntiforgeryToken]
    public class PropertyApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PropertyApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("AutoSaveDraft")]
        public async Task<IActionResult> AutoSaveDraft([FromBody] Property draftModel)
        {
            if (draftModel == null) return BadRequest("Geçersiz veri");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            Property existingDraft = null;

            // Eğer ID gelmişse o ID'li taslağı bul, gelmemişse yeni bir tane açması için null bırak
            if (draftModel.Id > 0)
            {
                existingDraft = await _context.Properties.FirstOrDefaultAsync(p => p.Id == draftModel.Id && p.UserId == userId);
            }

            if (existingDraft != null)
            {
                // Mevcut Taslağı Güncelle
                if (!string.IsNullOrEmpty(draftModel.Title)) existingDraft.Title = draftModel.Title;
                if (!string.IsNullOrEmpty(draftModel.Description)) existingDraft.Description = draftModel.Description;
                if (draftModel.Price > 0) existingDraft.Price = draftModel.Price;
                if (draftModel.StatusId > 0) existingDraft.StatusId = draftModel.StatusId;
                if (draftModel.TypeId > 0) existingDraft.TypeId = draftModel.TypeId;
                if (draftModel.DraftStep > 0) existingDraft.DraftStep = draftModel.DraftStep;

                existingDraft.State = ListingState.Draft;

                await _context.SaveChangesAsync();
                return Ok(new { success = true, id = existingDraft.Id, step = existingDraft.DraftStep, message = "Taslak başarıyla güncellendi." });
            }
            else
            {
                // Yeni Taslak Oluştur
                draftModel.UserId = userId;
                draftModel.State = ListingState.Draft;
                if (string.IsNullOrEmpty(draftModel.Title)) draftModel.Title = "Yeni Taslak İlan";

                _context.Properties.Add(draftModel);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, id = draftModel.Id, step = draftModel.DraftStep, message = "Yeni taslak oluşturuldu." });
            }
        }

        [HttpPost("DiscardDraft")]
        public async Task<IActionResult> DiscardDraft([FromQuery] int propertyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var draft = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId && p.UserId == userId && p.State == ListingState.Draft);
            if (draft != null)
            {
                _context.Properties.Remove(draft);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Taslak silindi." });
            }
            
            return NotFound(new { success = false, message = "Taslak bulunamadı." });
        }
    }
}
