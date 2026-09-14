using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Identity;
using GMK360.Core.Entities.B2B;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class B2BNetworkContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public B2BNetworkContactsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetCurrentAgencyId()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var consultant = await _context.AgencyConsultants
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive);
                
            return consultant?.AgencyId;
        }

        public async Task<IActionResult> Index(string type = "taseron")
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var query = _context.B2BNetworkContacts
                .Where(c => c.OwnerAgencyId == agencyId && !c.IsDeleted)
                .AsQueryable();

            if (type == "tedarikci") 
            {
                query = query.Where(c => c.SectorCategory == "Malzeme Tedarikçisi");
                ViewBag.ListType = "Tedarikçi Havuzu";
                ViewBag.Type = "tedarikci";
            } 
            else 
            {
                query = query.Where(c => c.SectorCategory != "Malzeme Tedarikçisi");
                ViewBag.ListType = "Taþeron Havuzu";
                ViewBag.Type = "taseron";
            }

            var contacts = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(contacts);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string type = "taseron")
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();
            ViewBag.Type = type;
            return View(new B2BNetworkContact());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(B2BNetworkContact model)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            ModelState.Remove("OwnerAgency");
            ModelState.Remove("RegisteredAgency");

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                model.OwnerAgencyId = agencyId.Value;
                model.AddedByUserId = user?.Id ?? "System";
                model.CreatedAt = DateTime.UtcNow;

                _context.B2BNetworkContacts.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { type = model.SectorCategory == "Malzeme Tedarikçisi" ? "tedarikci" : "taseron" });
            }

            return View(model);
        }
    }
}


