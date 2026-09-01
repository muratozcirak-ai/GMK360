using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class OfferController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OfferController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Inbox()
        {
            var user = await _userManager.GetUserAsync(User);
            // Gelen teklifler (İlanın sahibi olduğum)
            var receivedOffers = await _context.Offers
                                         .Include(o => o.Property)
                                         .Include(o => o.Buyer)
                                         .Where(o => o.SellerId == user.Id)
                                         .OrderByDescending(o => o.CreatedAt)
                                         .ToListAsync();

            ViewBag.SentOffers = await _context.Offers
                                         .Include(o => o.Property)
                                         .Include(o => o.Seller)
                                         .Where(o => o.BuyerId == user.Id)
                                         .OrderByDescending(o => o.CreatedAt)
                                         .ToListAsync();

            return View(receivedOffers);
        }

        [HttpPost]
        public async Task<IActionResult> MakeOffer(int propertyId, decimal offeredPrice)
        {
            var buyer = await _userManager.GetUserAsync(User);
            var property = await _context.Properties.FindAsync(propertyId);

            if (property == null || buyer.Id == property.UserId)
            {
                // Kendi ilanına teklif veremez
                return RedirectToAction("Detail", "Property", new { id = propertyId });
            }

            var offer = new Offer
            {
                PropertyId = propertyId,
                BuyerId = buyer.Id,
                SellerId = property.UserId,
                OfferedPrice = offeredPrice,
                Status = OfferStatus.Pending
            };

            _context.Offers.Add(offer);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Teklifiniz başarıyla satıcıya iletildi.";
            return RedirectToAction("Detail", "Property", new { id = propertyId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int offerId, OfferStatus status)
        {
            var user = await _userManager.GetUserAsync(User);
            var offer = await _context.Offers.FindAsync(offerId);

            if (offer != null && offer.SellerId == user.Id)
            {
                offer.Status = status;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Inbox));
        }
    }
}
