using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.B2B;
using System.Linq;
using System;

namespace GMK360.Web.Controllers
{
    public class PublicQuoteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublicQuoteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /PublicQuote/Submit?token=XYZ
        [HttpGet]
        public async Task<IActionResult> Submit(string token)
        {
            if (string.IsNullOrEmpty(token)) return NotFound();

            var invite = await _context.B2BQuoteInvites
                .Include(i => i.NetworkContact)
                .Include(i => i.QuoteRequest)
                    .ThenInclude(qr => qr.Items)
                        .ThenInclude(it => it.MaterialCatalog)
                .Include(i => i.QuoteRequest)
                    .ThenInclude(qr => qr.RequesterAgency)
                .Include(i => i.InviteItems)
                .FirstOrDefaultAsync(i => i.AccessToken == token);

            if (invite == null) return NotFound("Geçersiz veya süresi dolmuş davet linki.");

            return View(invite);
        }

        // POST: /PublicQuote/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string token, decimal? offeredPrice, string offerNotes, int[] itemIds, decimal[] itemPrices)
        {
            if (string.IsNullOrEmpty(token)) return NotFound();

            var invite = await _context.B2BQuoteInvites
                .Include(i => i.QuoteRequest)
                .Include(i => i.InviteItems)
                .FirstOrDefaultAsync(i => i.AccessToken == token);

            if (invite == null) return NotFound();

            invite.OfferedPrice = offeredPrice;
            invite.OfferNotes = offerNotes;
            invite.Status = QuoteInviteStatus.Submitted;
            invite.RespondedAt = DateTime.UtcNow;

            // Fiyatlari guncelle
            if (itemIds != null && itemPrices != null && itemIds.Length == itemPrices.Length)
            {
                for (int i = 0; i < itemIds.Length; i++)
                {
                    var inviteItem = invite.InviteItems.FirstOrDefault(x => x.B2BQuoteItemId == itemIds[i]);
                    if (inviteItem == null)
                    {
                        inviteItem = new B2BQuoteInviteItem
                        {
                            B2BQuoteInviteId = invite.Id,
                            B2BQuoteItemId = itemIds[i],
                            OfferedUnitPrice = itemPrices[i]
                        };
                        _context.B2BQuoteInviteItems.Add(inviteItem);
                    }
                    else
                    {
                        inviteItem.OfferedUnitPrice = itemPrices[i];
                    }
                }
            }

            // Acenteye (istegi yapan) bildirim gonder
            if(!string.IsNullOrEmpty(invite.QuoteRequest.RequesterUserId)) 
            {
                _context.UserNotifications.Add(new GMK360.Core.Entities.UserNotification
                {
                    ApplicationUserId = invite.QuoteRequest.RequesterUserId,
                    Title = $"Yeni Teklif Geldi: {invite.QuoteRequest.Title}",
                    Message = $"Tedarikçi teklif formunu doldurdu ve iletti. Tutar: {(offeredPrice.HasValue ? offeredPrice.Value.ToString("C2") : "Belirtilmedi")}",
                    LinkUrl = $"/B2BPurchasing/Details/{invite.QuoteRequestId}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return View("Success");
        }
    }
}
