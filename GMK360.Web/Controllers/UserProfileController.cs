using Microsoft.AspNetCore.Mvc;

namespace GMK360.Web.Controllers
{
    public class UserProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}