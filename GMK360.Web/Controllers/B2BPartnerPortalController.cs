using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.B2B;

namespace GMK360.Web.Controllers
{
    public class B2BPartnerPortalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public B2BPartnerPortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("B2BPartnerPortal/Invite/{token}")]
        public async Task<IActionResult> Invite(string token)
        {
            var invite = await _context.B2BQuoteInvites
                .Include(i => i.NetworkContact)
                .Include(i => i.QuoteRequest)
                    .ThenInclude(qr => qr.RequesterAgency)
                .FirstOrDefaultAsync(i => i.AccessToken == token);

            if (invite == null) return NotFound("Geçersiz veya süresi dolmuş davet linki.");

            dynamic projectInfo = null;
            if (invite.QuoteRequest.SourceModule == "PhaseZeroDocument")
            {
                var doc = await _context.ProjectLegalDocuments
                    .Include(d => d.ConstructionProject)
                    .FirstOrDefaultAsync(d => d.Id == invite.QuoteRequest.SourceReferenceId);

                if (doc != null && doc.ConstructionProject != null)
                {
                    projectInfo = new {
                        Name = doc.ConstructionProject.Name,
                        Address = doc.ConstructionProject.Address,
                        Lat = doc.ConstructionProject.Latitude,
                        Lng = doc.ConstructionProject.Longitude,
                        BuiltYear = "1970", 
                        Area = doc.ConstructionProject.TotalLandArea > 0 ? doc.ConstructionProject.TotalLandArea.ToString() : "1250",
                        Type = doc.ConstructionProject.ProjectType ?? "2 Blok Bitişik Nizam",
                        Units = "43 Daire, 2 Dükkan" 
                    };
                }
            }

            ViewBag.ProjectInfo = projectInfo;
            return View(invite);
        }

        [HttpPost("B2BPartnerPortal/SubmitQuote")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuote(int inviteId, decimal offeredPrice)
        {
            var invite = await _context.B2BQuoteInvites.FirstOrDefaultAsync(i => i.Id == inviteId);
            if (invite == null) return NotFound();

            invite.OfferedPrice = offeredPrice;
            invite.Status = QuoteInviteStatus.Submitted;
            invite.RespondedAt = DateTime.UtcNow;

            var quoteReq = await _context.B2BQuoteRequests.FirstOrDefaultAsync(q => q.Id == invite.QuoteRequestId);
            if (quoteReq != null && quoteReq.SourceModule == "PhaseZeroDocument")
            {
                var doc = await _context.ProjectLegalDocuments.FirstOrDefaultAsync(d => d.Id == quoteReq.SourceReferenceId);
                if (doc != null) doc.Status = "Teklif Geldi";
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Teklifiniz başarıyla iletildi. Dengeham İnşaat yetkilisi değerlendirdikten sonra size geri dönüş yapacaktır.";
            return RedirectToAction("Invite", new { token = invite.AccessToken });
        }

        [HttpPost("B2BPartnerPortal/RequestInspection")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestInspection(int inviteId)
        {
            var invite = await _context.B2BQuoteInvites.FirstOrDefaultAsync(i => i.Id == inviteId);
            if (invite == null) return NotFound();

            // Sadece not düşüyoruz, durumu hala Pending ama satınalmacı notu görecek.
            invite.OfferNotes = "KEŞİF TALEBİ: Firma fiyat vermeden önce sahayı görmek / randevu talep ediyor.";
            invite.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["InfoMessage"] = "Keşif ve saha inceleme talebiniz firmaya iletildi. Yetkililer randevu için sizinle iletişime geçecektir.";
            return RedirectToAction("Invite", new { token = invite.AccessToken });
        }
    }
}
