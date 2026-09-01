using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GMK360.Data.Contexts;
using System.Linq;

namespace GMK360.Web.Controllers
{
    [Authorize] // Ileride Role/Policy eklenecek (Sadece Kurumsal ve Danismanlar)
    public class RealtorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RealtorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MultiPost()
        {
            return View();
        }
    }
}
