using Microsoft.AspNetCore.Mvc;

namespace GMK360.Web.Controllers
{
    public class AdminRequestsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}