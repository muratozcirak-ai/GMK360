using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class MaterialListController : Controller
    {
        // Usta Ekranı: Malzeme Listesi Oluşturma
        [Authorize(Roles = "Admin,ServiceProvider")]
        public IActionResult Index()
        {
            return View();
        }

        // Nalbur Ekranı: Gelen Talepler
        [Authorize(Roles = "Admin,Supplier")]
        public IActionResult IncomingRequests()
        {
            return View();
        }
    }
}
