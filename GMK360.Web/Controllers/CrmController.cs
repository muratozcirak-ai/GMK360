using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "Admin,Emlakci,Danisman")]
    public class CrmController : Controller
    {
        public IActionResult Rehber()
        {
            return View();
        }

        public IActionResult Ajanda()
        {
            return View();
        }

        public IActionResult Talepler()
        {
            return View();
        }

        public IActionResult KiralamaTakip()
        {
            return View();
        }
    }
}
