using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Helpers;

namespace GMK360.Web.Controllers
{
    public class UstaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UstaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DetailBySlug(string seoSlug)
        {
            if (string.IsNullOrEmpty(seoSlug)) return NotFound();

            var parts = seoSlug.Split('-');
            if (parts.Length > 0 && int.TryParse(parts.Last(), out int id))
            {
                return await Detail(id, isFromSlug: true);
            }
            return NotFound();
        }

        public async Task<IActionResult> Detail(int id, bool isFromSlug = false)
        {
            if (!isFromSlug)
            {
                var prov = await _context.ServiceProviders
                    .Include(p => p.Areas).ThenInclude(a => a.District)
                    .FirstOrDefaultAsync(p => p.Id == id);
                
                if (prov == null) return NotFound();
                
                var distStr = prov.Areas?.FirstOrDefault()?.District?.Name.ToSeoUrl() ?? "bolge";
                var nameStr = prov.BusinessName.ToSeoUrl();
                var slug = $"{distStr}-{nameStr}-{prov.Id}";
                
                return RedirectPermanent($"/usta/{slug}");
            }

            // Normal Detail (Veritabanından asıl çekim, View'a aktarım vs.)
            var model = await _context.ServiceProviders
                .Include(p => p.Areas).ThenInclude(a => a.District)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (model == null) return NotFound();

            return View(model);
        }
    }
}
