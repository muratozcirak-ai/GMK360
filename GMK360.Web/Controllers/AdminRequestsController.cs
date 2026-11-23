using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? module = null, string? status = null, string? type = null)
        {
            var baseQuery = _context.PlatformFeedbacks
                .Where(f => !f.IsDeleted);

            ViewBag.TotalCount = await baseQuery.CountAsync();
            ViewBag.NewCount = await baseQuery.CountAsync(f => f.Status == "Yeni");
            ViewBag.SiteCount = await baseQuery.CountAsync(f => f.ModuleArea == "SiteYonetimi" || f.ModuleArea == "SakinPortal");
            ViewBag.ConstructionCount = await baseQuery.CountAsync(f => f.ModuleArea == "InsaatYonetimi");
            ViewBag.BugCount = await baseQuery.CountAsync(f => f.FeedbackType == "Hata" && f.Status != "Cozuldu" && f.Status != "Kapandi");

            var query = baseQuery.AsQueryable();

            if (!string.IsNullOrWhiteSpace(module))
            {
                if (module == "SiteGroup")
                {
                    query = query.Where(f => f.ModuleArea == "SiteYonetimi" || f.ModuleArea == "SakinPortal");
                }
                else
                {
                    query = query.Where(f => f.ModuleArea == module);
                }
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(f => f.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(f => f.FeedbackType == type);
            }

            var items = await query
                .OrderByDescending(f => f.Status == "Yeni")
                .ThenByDescending(f => f.CreatedAt)
                .Take(200)
                .ToListAsync();

            ViewBag.FilterModule = module ?? "";
            ViewBag.FilterStatus = status ?? "";
            ViewBag.FilterType = type ?? "";

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFeedback(int id, string status, string? adminNote, string? filterModule, string? filterStatus)
        {
            var item = await _context.PlatformFeedbacks.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
            if (item == null)
            {
                TempData["ErrorMessage"] = "Geri bildirim kaydı bulunamadı.";
                return RedirectToAction(nameof(Index), new { module = filterModule, status = filterStatus });
            }

            item.Status = string.IsNullOrWhiteSpace(status) ? item.Status : status.Trim();
            item.AdminNote = string.IsNullOrWhiteSpace(adminNote) ? null : adminNote.Trim();
            item.UpdatedAt = DateTime.UtcNow;
            if (item.Status == "Cozuldu" || item.Status == "Kapandi")
            {
                item.ResolvedAt ??= DateTime.UtcNow;
            }
            else
            {
                item.ResolvedAt = null;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Geri bildirim #{item.Id} güncellendi ({item.Status}).";
            return RedirectToAction(nameof(Index), new { module = filterModule, status = filterStatus });
        }
    }
}
