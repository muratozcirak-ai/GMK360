using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.B2B;

namespace GMK360.Web.Controllers
{
    public class OfferController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OfferController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Submit(int id)
        {
            var invite = await _context.B2BQuoteInvites
                .Include(i => i.NetworkContact)
                .Include(i => i.QuoteRequest)
                    .ThenInclude(qr => qr.RequesterAgency)
                .Include(i => i.QuoteRequest)
                    .ThenInclude(qr => qr.Items)
                        .ThenInclude(item => item.MaterialCatalog)
                .Include(i => i.InviteItems)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invite == null) return NotFound();

            return View(invite);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitPrice(int id, decimal[] unitPrices, int[] itemIds, bool isVatIncluded, string offerNotes)
        {
            var invite = await _context.B2BQuoteInvites
                .Include(i => i.QuoteRequest)
                    .ThenInclude(qr => qr.Items)
                .FirstOrDefaultAsync(i => i.Id == id);
                
            if (invite == null) return NotFound();

            decimal grandTotal = 0;

            for (int i = 0; i < itemIds.Length; i++)
            {
                var reqItem = invite.QuoteRequest.Items.FirstOrDefault(qi => qi.Id == itemIds[i]);
                if (reqItem != null)
                {
                    var price = unitPrices[i];
                    grandTotal += (price * reqItem.Quantity);

                    var inviteItem = new B2BQuoteInviteItem
                    {
                        B2BQuoteInviteId = invite.Id,
                        B2BQuoteItemId = reqItem.Id,
                        OfferedUnitPrice = price,
                        IsVatIncluded = isVatIncluded,
                        VatRate = 20
                    };
                    _context.B2BQuoteInviteItems.Add(inviteItem);
                }
            }

            invite.OfferedPrice = grandTotal;
            invite.IsVatIncludedGlobally = isVatIncluded;
            invite.OfferNotes = offerNotes ?? "";
            invite.Status = QuoteInviteStatus.Submitted;
            invite.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Teklifiniz başarıyla iletildi! Teşekkür ederiz.";
            return RedirectToAction("Submit", new { id = id });
        }
    }
}

