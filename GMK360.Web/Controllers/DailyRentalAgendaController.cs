using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class DailyRentalAgendaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // TODO: AJAX endpointleri buraya gelecek (GetReservations vb.)
    }
}
