using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    public class SiteDiaryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Templates()
        {
            return View();
        }
    }
}
