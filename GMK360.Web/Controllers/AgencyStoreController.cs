using Microsoft.AspNetCore.Mvc;
using GMK360.Data.Contexts;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    public class AgencyStoreController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AgencyStoreController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string subdomain)
        {
            if (string.IsNullOrEmpty(subdomain))
            {
                return NotFound();
            }

            var agency = _context.Set<GMK360.Core.Entities.Agency>()
                .Include(a => a.AgencyConsultants)
                .ThenInclude(c => c.User)
                .FirstOrDefault(a => a.Subdomain.ToLower() == subdomain.ToLower());

            if (agency == null)
            {
                return NotFound("Böyle bir emlak ofisi bulunamadý.");
            }

            // TODO: Ýlanlarý da getireceðiz
            return View(agency);
        }
    }
}
