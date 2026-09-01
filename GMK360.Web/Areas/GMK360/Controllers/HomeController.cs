using Microsoft.AspNetCore.Mvc;

namespace GMK360.Web.Areas.GMK360.Controllers
{
    [Area("GMK360")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
