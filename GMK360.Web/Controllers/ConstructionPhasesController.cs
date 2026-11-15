using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class ConstructionPhasesController : Controller
    {
        public IActionResult Active()
        {
            return View();
        }
    }
}
