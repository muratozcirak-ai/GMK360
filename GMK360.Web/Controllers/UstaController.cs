using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Helpers;
using GMK360.Core.Entities;

namespace GMK360.Web.Controllers
{
    public class UstaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UstaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DetailBySlug(string seoSlug)
        {
            if (string.IsNullOrEmpty(seoSlug)) return NotFound();

            var parts = seoSlug.Split('-');
            if (parts.Length > 0 && int.TryParse(parts.Last(), out int id))
            {
                return await Detail(id, isFromSlug: true);
            }
            return NotFound();
        }

        public async Task<IActionResult> Detail(int id, bool isFromSlug = false)
        {
            if (!isFromSlug)
            {
                var prov = await _context.ServiceProviders
                    .Include(p => p.Areas).ThenInclude(a => a.District)
                    .FirstOrDefaultAsync(p => p.Id == id);
                
                if (prov == null) return NotFound();
                
                var distStr = prov.Areas?.FirstOrDefault()?.District?.Name.ToSeoUrl() ?? "bolge";
                var nameStr = prov.BusinessName.ToSeoUrl();
                var slug = $"{distStr}-{nameStr}-{prov.Id}";
                
                return RedirectPermanent($"/usta/{slug}");
            }

            var model = await _context.ServiceProviders
                .Include(p => p.Areas).ThenInclude(a => a.District)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (model == null) return NotFound();

            return View(model);
        }

        // --- YENİ EKLENEN AKSİYONLAR (APPEND-ONLY) ---

        // Bireysel Kullanıcının veya Yöneticinin İhale/İş Talebi Açtığı Ekran
        [HttpGet]
        public IActionResult TalepAc()
        {
            // Kullanıcı usta veya malzeme aradığında buraya düşer.
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TalepAc(JobRequest model)
        {
            // İleride kullanıcı doğrulaması (Auth) eklenecek, şimdilik UI'ın kaydetme mantığını simüle ediyoruz.
            // Arka planda Ajanın analiz edeceği serbest metin bu model.Description üzerinden gelecek.
            return RedirectToAction("Index", "PublicRealEstate");
        }

        // Usta veya Malzeme Tedarikçisinin havuza girdiği kayıt ekranı
        [HttpGet]
        public IActionResult Basvuru()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Basvuru(GMK360.Core.Entities.ServiceProvider model)
        {
            // Kurumsal hizmet, hafriyat, yıkım veya tedarikçi başvurusu
            return RedirectToAction("Index", "PublicRealEstate");
        }
    }
}

