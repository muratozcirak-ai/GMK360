using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using System.Linq;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminSeoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminSeoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var settings = _context.SeoSettings.ToList();
            return View(settings);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(SeoSetting model)
        {
            if (ModelState.IsValid)
            {
                if (model.PagePath == "/") model.PagePath = "/Home/Index";
                _context.SeoSettings.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "SEO Ayarý Eklendi";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var model = _context.SeoSettings.Find(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(SeoSetting model)
        {
            if (ModelState.IsValid)
            {
                var exist = _context.SeoSettings.Find(model.Id);
                if (exist != null)
                {
                    exist.PagePath = model.PagePath == "/" ? "/Home/Index" : model.PagePath;
                    exist.Title = model.Title;
                    exist.Description = model.Description;
                    exist.Keywords = model.Keywords;
                    exist.OgImage = model.OgImage;
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "SEO Ayarý Güncellendi";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var exist = _context.SeoSettings.Find(id);
            if (exist != null)
            {
                _context.SeoSettings.Remove(exist);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
