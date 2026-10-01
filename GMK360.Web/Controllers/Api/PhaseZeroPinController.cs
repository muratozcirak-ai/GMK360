using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;

namespace GMK360.Web.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PhaseZeroPinController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PhaseZeroPinController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("ApproveQuote")]
        public async Task<IActionResult> ApproveQuote([FromForm] int docId, [FromForm] string pin)
        {
            // Sabit Demo PIN kontrolü
            if (pin != "123456")
            {
                return BadRequest("Geçersiz Yönetici PIN Kodu.");
            }

            var doc = await _context.ProjectLegalDocuments.FirstOrDefaultAsync(d => d.Id == docId);
            if (doc == null) return NotFound("İlgili evrak bulunamadı.");

            // Belgeye bağlı B2BQuoteRequest'i bul
            var quoteReq = await _context.B2BQuoteRequests
                .Include(q => q.Invites)
                .FirstOrDefaultAsync(q => q.SourceModule == "PhaseZeroDocument" && q.SourceReferenceId == docId);

            if (quoteReq == null || !quoteReq.Invites.Any(i => i.Status == Core.Entities.B2B.QuoteInviteStatus.Submitted))
            {
                return BadRequest("Bu işlem için geçerli/gönderilmiş bir teklif bulunamadı.");
            }

            // En düşük teklifi bul
            var bestInvite = quoteReq.Invites
                .Where(i => i.Status == Core.Entities.B2B.QuoteInviteStatus.Submitted && i.OfferedPrice.HasValue)
                .OrderBy(i => i.OfferedPrice)
                .FirstOrDefault();

            if (bestInvite == null) return BadRequest("Geçerli fiyat içeren teklif yok.");

            // Maliyeti Evrağa yaz
            doc.DocumentFee = bestInvite.OfferedPrice;
            doc.Status = "Alındı"; // Teklif onaylandığı için işlemi tamamlandı sayabiliriz

            // Diğer teklifleri reddedilmiş yap, kazananı onayla
            foreach (var invite in quoteReq.Invites)
            {
                if (invite.Id == bestInvite.Id)
                {
                    invite.Status = Core.Entities.B2B.QuoteInviteStatus.Accepted;
                }
                else
                {
                    invite.Status = Core.Entities.B2B.QuoteInviteStatus.Rejected;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, bestPrice = bestInvite.OfferedPrice });
        }
    }
}
