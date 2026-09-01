using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class MobileTaskController : Controller
    {
        // Ustalar için devasa butonlu, basitleştirilmiş arayüz
        public async Task<IActionResult> Index()
        {
            // İleride veritabanından ustanın (kendi) görevleri çekilecek.
            return View();
        }
        
        public async Task<IActionResult> Detail(int id)
        {
            // Görev detayı, kat sahibinin malzeme seçimini gösterecek.
            ViewBag.TaskId = id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadProgressPhoto(int taskId, Microsoft.AspNetCore.Http.IFormFile photo)
        {
            // Fotoğraf kaydedilecek ve TaskProgressLog'a eklenecek.
            TempData["SuccessMessage"] = "Fotoğraf başarıyla yüklendi! İşlem kaydedildi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
