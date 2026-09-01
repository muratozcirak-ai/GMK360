using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GMK360.Core.Interfaces;
using GMK360.Core.Entities;

namespace GMK360.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Admin")] // Sadece yetkililer
    public class ArticleController : Controller
    {
        private readonly IArticleService _articleService;

        public ArticleController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        // GET: Admin/Article
        public async Task<IActionResult> Index()
        {
            var articles = await _articleService.GetAllArticlesAsync(includeUnpublished: true);
            return View(articles);
        }

        // GET: Admin/Article/Create
        public IActionResult Create()
        {
            return View(new Article());
        }

        // POST: Admin/Article/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Article model)
        {
            if (ModelState.IsValid)
            {
                await _articleService.CreateArticleAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Admin/Article/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null) return NotFound();
            return View(article);
        }

        // POST: Admin/Article/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Article model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _articleService.UpdateArticleAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: Admin/Article/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _articleService.DeleteArticleAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
