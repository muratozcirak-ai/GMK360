using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class DailyRentalController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
