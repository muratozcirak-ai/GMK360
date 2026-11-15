using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using GMK360.Data.Contexts;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class LaborMobilizationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LaborMobilizationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Veritabanındaki Proje DNA Profili / Malzeme Reçetesi başlıklarını dinamik çeker
            var realCategories = _context.MaterialRecipes
                                         .Select(m => m.WorkItemName)
                                         .Distinct()
                                         .ToList();

            // Eğer henüz reçete yoksa boş dönmesin diye dummy 1-2 şey ekleyebiliriz ama 
            // ustamızın istediği gibi sistemden gelenler gösterilecek.
            if (!realCategories.Any())
            {
                realCategories.Add("DNA Profilinden Bekleniyor (Fayans, Kalıp vs.)");
            }

            ViewBag.Categories = realCategories;
            return View();
        }
    }
}
