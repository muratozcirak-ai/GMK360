using Microsoft.AspNetCore.Mvc;

namespace GMK360.Web.Controllers
{
    public class PresentationController : Controller
    {
        public IActionResult B2BQuotes()
        {
            return View();
        }

        public IActionResult FeasibilitySimulator()
        {
            return View();
        }
    }
}
