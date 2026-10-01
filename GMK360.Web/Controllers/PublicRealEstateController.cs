using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;

namespace GMK360.Web.Controllers
{
    [Route("Emlak")]
    public class PublicRealEstateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublicRealEstateController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ana Arama Portalı Girişi (Örn: gmk360.com/Emlak)
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var showcaseProperties = await _context.Properties
                .Include(p => p.Building)
                .Where(p => p.State == GMK360.Core.Entities.ListingState.Active)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();

            return View(showcaseProperties);
        }

        // SEO Dostu Arama Rotası: Örn: /Emlak/Kiralik/Istanbul-Kadikoy
        [HttpGet("{status}/{locationSlug}")]
        public async Task<IActionResult> Search(string status, string locationSlug)
        {
            ViewBag.SearchStatus = status; // satilik, kiralik vb.
            ViewBag.Location = locationSlug;
            
            var query = _context.Properties.Include(p => p.Building).Where(p => p.State == GMK360.Core.Entities.ListingState.Active);
            
            if (status.ToLower().Contains("kiralik")) {
                query = query.Where(p => p.StatusId == 2);
            }
            
            var results = await query.ToListAsync();
            return View("Search", results);
        }

        // İlan Detay Sayfası: Örn: /Emlak/Detay/1234/deniz-manzarali-3-1
        [HttpGet("Detay/{id}/{slug?}")]
        public async Task<IActionResult> Detail(int id, string slug = null)
        {
            var property = await _context.Properties
                .Include(p => p.Building)
                .FirstOrDefaultAsync(p => p.Id == id && p.State == GMK360.Core.Entities.ListingState.Active);

            if (property == null) return NotFound("İlan bulunamadı veya yayından kaldırılmış.");

            return View(property);
        }
    }
}
