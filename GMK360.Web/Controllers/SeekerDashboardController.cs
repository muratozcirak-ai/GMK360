using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;

namespace GMK360.Web.Controllers
{
    // BİREYSEL ARAYAN (SEEKER) KONTROL PANELİ
    public class SeekerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SeekerDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. ÖZET PANELİM (Dashboard Widgets)
        public IActionResult Index()
        {
            // İleride [Authorize] ile User.Identity.Name alınacak.
            return View();
        }

        // 2. KİŞİSEL EMLAK AJANDASI (İzole Veri)
        public IActionResult Agenda()
        {
            return View();
        }

        // 3. FAVORİLER VE TOPLU KIYASLAMA (Split-Screen / Grid)
        public IActionResult Favorites()
        {
            return View();
        }

        // 4. TEKLİFLERİM (Verilen Fiyat Teklifleri)
        public IActionResult Offers()
        {
            return View();
        }
        
        // 5. ASENKRON ARAMA KANCASI (Ajax Live Search)
        [HttpGet]
        public IActionResult LiveSearch(string criteria)
        {
            // Sayfa yenilenmeden sonuçları PartialView olarak döner
            return PartialView("_LiveSearchResults");
        }
    }
}
