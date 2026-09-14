using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Marketplace;
using GMK360.Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class MarketplaceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MarketplaceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Marketplace (Açık Pazar Ana Sayfası)
        public async Task<IActionResult> Index(string filter = "all")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var query = _context.MarketplaceJobs
                .Include(j => j.CreatedByUser)
                .Include(j => j.Bids)
                .Where(j => j.IsActive);

            if (filter == "my")
            {
                query = query.Where(j => j.CreatedByUserId == user.Id);
            }

            var jobs = await query.OrderByDescending(j => j.CreatedAt).ToListAsync();
            
            ViewBag.CurrentFilter = filter;
            ViewBag.CurrentUserId = user.Id;
            
            return View(jobs);
        }

        // GET: Marketplace/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MarketplaceJob model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (ModelState.IsValid)
            {
                model.CreatedByUserId = user.Id;
                model.CreatedAt = DateTime.UtcNow;
                model.IsActive = true;

                _context.MarketplaceJobs.Add(model);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "İlanınız açık pazara eklendi!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Marketplace/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var job = await _context.MarketplaceJobs
                .Include(j => j.CreatedByUser)
                .Include(j => j.Bids)
                    .ThenInclude(b => b.BidderUser)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null) return NotFound();

            ViewBag.CurrentUserId = user.Id;
            return View(job);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitBid(int jobId, decimal offeredPrice, string message)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var job = await _context.MarketplaceJobs.FindAsync(jobId);
            if (job == null) return NotFound();

            var bid = new MarketplaceBid
            {
                MarketplaceJobId = jobId,
                BidderUserId = user.Id,
                OfferedPrice = offeredPrice,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            _context.MarketplaceBids.Add(bid);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Teklifiniz ve mesajınız başarıyla iletildi!";
            return RedirectToAction(nameof(Detail), new { id = jobId });
        }
    }
}
