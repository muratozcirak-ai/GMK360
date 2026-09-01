using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class DocumentArchiveController : Controller
    {
        public IActionResult Index(string context)
        {
            return View();
        }
    }
}
