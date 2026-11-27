using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using GMK360.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    /// <summary>
    /// Sadece Süper Admin / Admin tarafından merkezi olarak KMK mevzuat rehberleri ve Yönetici El Kitabı bölümlerinin
    /// eklenip güncellenebildiği CMS denetleyicisi. Site yöneticileri bu içeriklere müdahale edemez.
    /// </summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminMevzuatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminMevzuatController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? q = null)
        {
            await KmkGuideSeederService.EnsureKmkGuidesSeededAsync(_context);

            var query = _context.Articles.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(a => a.Title.Contains(term) || a.Category.Contains(term) || a.Slug.Contains(term));
            }

            var list = await query
                .OrderBy(a => a.IsMembersOnly)
                .ThenByDescending(a => a.UpdatedAt ?? a.PublishedAt ?? a.CreatedAt)
                .ToListAsync();

            ViewBag.SearchQuery = q ?? "";
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(
            int id,
            string title,
            string? slug,
            string category,
            string? seoTags,
            string htmlContent,
            bool isMembersOnly,
            bool isPublished)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(htmlContent))
            {
                TempData["ErrorMessage"] = "Başlık ve içerik metni zorunludur.";
                return RedirectToAction(nameof(Index));
            }

            string normalizedSlug = BuildSlug(string.IsNullOrWhiteSpace(slug) ? title : slug);
            bool duplicate = await _context.Articles.AnyAsync(a => a.Slug == normalizedSlug && a.Id != id);
            if (duplicate)
            {
                normalizedSlug = $"{normalizedSlug}-{DateTime.UtcNow:yyyyMMddHHmmss}";
            }

            if (id > 0)
            {
                var existing = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id);
                if (existing == null)
                {
                    TempData["ErrorMessage"] = "Güncellenecek mevzuat kaydı bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                existing.Title = title.Trim();
                existing.Slug = normalizedSlug;
                existing.Category = string.IsNullOrWhiteSpace(category) ? KmkGuideSeederService.PublicCategory : category.Trim();
                existing.SeoTags = seoTags?.Trim();
                existing.HtmlContent = htmlContent;
                existing.IsMembersOnly = isMembersOnly;
                if (isPublished && !existing.IsPublished && existing.PublishedAt == null)
                {
                    existing.PublishedAt = DateTime.UtcNow;
                }
                existing.IsPublished = isPublished;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"'{existing.Title}' merkezi olarak güncellendi. Tüm halka açık ve üye el kitabı sayfaları anında yenilendi.";
            }
            else
            {
                var created = new Article
                {
                    Title = title.Trim(),
                    Slug = normalizedSlug,
                    Category = string.IsNullOrWhiteSpace(category) ? KmkGuideSeederService.HandbookCategory : category.Trim(),
                    SeoTags = seoTags?.Trim(),
                    HtmlContent = htmlContent,
                    IsMembersOnly = isMembersOnly,
                    IsPublished = isPublished,
                    PublishedAt = isPublished ? DateTime.UtcNow : null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Articles.Add(created);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"'{created.Title}' merkezi mevzuat havuzuna eklendi.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(int id)
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article != null)
            {
                article.IsPublished = !article.IsPublished;
                if (article.IsPublished && article.PublishedAt == null)
                {
                    article.PublishedAt = DateTime.UtcNow;
                }
                article.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"'{article.Title}' yayım durumu güncellendi.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article != null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kayıt merkezi havuzdan kaldırıldı.";
            }
            return RedirectToAction(nameof(Index));
        }

        private static string BuildSlug(string input)
        {
            string s = input.Trim().ToLowerInvariant()
                .Replace("ç", "c").Replace("ğ", "g").Replace("ı", "i")
                .Replace("ö", "o").Replace("ş", "s").Replace("ü", "u");
            s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
            s = Regex.Replace(s, @"\s+", "-").Trim('-');
            return string.IsNullOrWhiteSpace(s) ? "kmk-rehber-" + DateTime.UtcNow.Ticks : s;
        }
    }
}
