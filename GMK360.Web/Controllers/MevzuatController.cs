using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using GMK360.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    /// <summary>
    /// Halka açık KMK Mevzuat, Bina Yönetimi Rehberi ve Blog kütüphanesi.
    /// Merkezi veritabanından (Articles) beslenir; tüm ziyaretçiler için salt okunurdur.
    /// </summary>
    [AllowAnonymous]
    public class MevzuatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MevzuatController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/Mevzuat")]
        [HttpGet("/Mevzuat/Index")]
        public async Task<IActionResult> Index(string? q = null, string? scope = null)
        {
            await KmkGuideSeederService.EnsureKmkGuidesSeededAsync(_context);

            var query = _context.Articles
                .AsNoTracking()
                .Where(a => a.IsPublished);

            if (string.Equals(scope, "public", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(a => !a.IsMembersOnly);
            }
            else if (string.Equals(scope, "handbook", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(a => a.IsMembersOnly);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(a =>
                    a.Title.Contains(term) ||
                    (a.SeoTags != null && a.SeoTags.Contains(term)) ||
                    a.Category.Contains(term));
            }

            var articles = await query
                .OrderBy(a => a.IsMembersOnly)
                .ThenBy(a => a.PublishedAt ?? a.CreatedAt)
                .ToListAsync();

            ViewBag.SearchQuery = q;
            ViewBag.Scope = string.IsNullOrWhiteSpace(scope) ? "all" : scope.ToLowerInvariant();
            ViewBag.IsAuthenticated = User?.Identity?.IsAuthenticated == true;
            return View(articles);
        }

        [HttpGet("/Mevzuat/{slug}")]
        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return RedirectToAction(nameof(Index));
            }

            await KmkGuideSeederService.EnsureKmkGuidesSeededAsync(_context);

            var article = await _context.Articles
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Slug == slug && a.IsPublished);

            if (article == null)
            {
                return NotFound();
            }

            bool isAuthenticated = User?.Identity?.IsAuthenticated == true;
            ViewBag.IsAuthenticated = isAuthenticated;
            ViewBag.IsLockedPreview = article.IsMembersOnly && !isAuthenticated;
            ViewBag.RelatedArticles = await _context.Articles
                .AsNoTracking()
                .Where(a => a.IsPublished && a.Id != article.Id)
                .OrderBy(a => a.IsMembersOnly)
                .ThenBy(a => a.PublishedAt ?? a.CreatedAt)
                .Take(6)
                .ToListAsync();

            return View(article);
        }
    }
}
