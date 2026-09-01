using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GMK360.Core.Interfaces;
using GMK360.Core.Entities;

namespace GMK360.Web.Controllers
{
    [AllowAnonymous] // Ziyaretçilere açık (Public View)
    public class BlogController : Controller
    {
        private readonly IArticleService _articleService;

        public BlogController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        // GET: /Blog
        public async Task<IActionResult> Index()
        {
            // Sadece yayında olan makaleleri getirir
            var articles = await _articleService.GetAllArticlesAsync(includeUnpublished: false);
            return View(articles); // Views/Blog/Index.cshtml
        }

        // GET: /Blog/{slug}
        [Route("Blog/{slug}")]
        public async Task<IActionResult> DetailBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return RedirectToAction("Index");
            }

            var article = await _articleService.GetArticleBySlugAsync(slug);
            if (article == null)
            {
                return NotFound();
            }

            // CTA Pazarlama Aracı: Makale üyelere özelse üye olmaya yönlendir
            if (article.IsMembersOnly && !User.Identity.IsAuthenticated)
            {
                TempData["MarketingMessage"] = "Bu makalenin tamamını ve vergi avantajları rehberini okumak için GMK360 platformuna ücretsiz katılın!";
                return RedirectToAction("Register", "Account");
            }

            // View içerisinde article.Author üzerinden Danışman Kartviziti oluşturulacak
            return View(article); 
        }
    }
}
