using Microsoft.AspNetCore.Mvc;
using System.Linq;
using GMK360.Data.Contexts;

namespace GMK360.Web.Controllers
{
    public class LaborMobilizationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LaborMobilizationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Veritabanýndaki Proje DNA Profili / Malzeme Reçetesi baþlýklarýný dinamik çeker
            var realCategories = _context.MaterialRecipes
                                         .Select(m => m.WorkItemName)
                                         .Distinct()
                                         .ToList();

            // Eðer henüz reçete yoksa boþ dönmesin diye dummy 1-2 þey ekleyebiliriz ama 
            // ustamýzýn istediði gibi sistemden gelenler gösterilecek.
            if (!realCategories.Any())
            {
                realCategories.Add("DNA Profilinden Bekleniyor (Fayans, Kalýp vs.)");
            }

            ViewBag.Categories = realCategories;
            return View();
        }
    }
}
