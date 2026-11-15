using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    [Authorize]
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
