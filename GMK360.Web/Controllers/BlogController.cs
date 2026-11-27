using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using GMK360.Web.Services;

namespace GMK360.Web.Controllers
{
    [AllowAnonymous] // Ziyaretçilere açık (Public View)
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Blog
        public async Task<IActionResult> Index(string? q = null, string? scope = null)
        {
            await KmkGuideSeederService.EnsureKmkGuidesSeededAsync(_context);
            return RedirectToAction("Index", "Mevzuat", new { q, scope });
        }

        // GET: /Blog/{slug}
        [Route("Blog/{slug}")]
        public async Task<IActionResult> DetailBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return RedirectToAction("Index", "Mevzuat");
            }

            await KmkGuideSeederService.EnsureKmkGuidesSeededAsync(_context);
            return RedirectToAction("Detail", "Mevzuat", new { slug });
        }
    }
}
