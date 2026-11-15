using Microsoft.AspNetCore.Mvc;
using GMK360.Data.Contexts;
using System.Linq;
using System.Security.Claims;
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

        public IActionResult Index(string? subdomain, int? agencyId)
        {
            GMK360.Core.Entities.Agency? agency = null;

            if (agencyId.HasValue && agencyId.Value > 0)
            {
                agency = _context.Set<GMK360.Core.Entities.Agency>()
                    .Include(a => a.AgencyConsultants)
                        .ThenInclude(c => c.User)
                    .FirstOrDefault(a => a.Id == agencyId.Value && !a.IsDeleted);
            }

            if (agency == null && !string.IsNullOrWhiteSpace(subdomain))
            {
                var normalized = subdomain.Trim().ToLower();
                agency = _context.Set<GMK360.Core.Entities.Agency>()
                    .Include(a => a.AgencyConsultants)
                        .ThenInclude(c => c.User)
                    .FirstOrDefault(a => !a.IsDeleted && a.Subdomain != null && a.Subdomain.ToLower() == normalized);
            }

            if (agency == null && User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userConsultant = _context.AgencyConsultants.FirstOrDefault(ac => ac.UserId == userId && !ac.IsDeleted);
                if (userConsultant != null)
                {
                    agency = _context.Set<GMK360.Core.Entities.Agency>()
                        .Include(a => a.AgencyConsultants)
                            .ThenInclude(c => c.User)
                        .FirstOrDefault(a => a.Id == userConsultant.AgencyId && !a.IsDeleted);
                }
            }

            if (agency == null)
            {
                agency = _context.Set<GMK360.Core.Entities.Agency>()
                    .Include(a => a.AgencyConsultants)
                        .ThenInclude(c => c.User)
                    .FirstOrDefault(a => !a.IsDeleted);
            }

            if (agency == null)
            {
                return NotFound("Kurumsal firma kaydı bulunamadı.");
            }

            var showcaseProjects = _context.ConstructionProjects
                .Where(p => p.AgencyId == agency.Id && !p.IsDeleted && p.IsPublishedOnWeb)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            ViewBag.ShowcaseProjects = showcaseProjects;
            return View(agency);
        }
    }
}
