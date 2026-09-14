using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Identity;
using System.Threading.Tasks;
using System.Linq;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "InsaatFirmasi,Corporate,Admin")]
    public class AgencyPhonebookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AgencyPhonebookController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            var userId = _userManager.GetUserId(User);
            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == userId);
            return consultant?.AgencyId;
        }

        public async Task<IActionResult> Index(byte? type)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var query = _context.AgencyPhonebooks.Where(p => p.AgencyId == agencyId);
            
            if (type.HasValue)
            {
                query = query.Where(p => p.ContactType == type.Value);
                ViewBag.ActiveFilter = type.Value;
            }
            else
            {
                ViewBag.ActiveFilter = 0;
            }

            var contacts = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return View(contacts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AgencyPhonebook model)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            model.AgencyId = agencyId.Value;
            model.CreatedAt = System.DateTime.UtcNow;
            
            if (ModelState.IsValid)
            {
                _context.AgencyPhonebooks.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kiþi/Firma rehbere eklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Lütfen zorunlu alanlarý doldurun.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SendInvite(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            var contact = await _context.AgencyPhonebooks.FirstOrDefaultAsync(c => c.Id == id && c.AgencyId == agencyId);
            
            if (contact == null) return NotFound();

            // Gerçek bir sistemde burada SMS veya E-posta tetiklenir
            // Þimdilik sadece toast mesajý vereceðiz
            
            TempData["SuccessMessage"] = $"{contact.Name} adlý kiþiye sisteme katýlým daveti gönderildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            var contact = await _context.AgencyPhonebooks.FirstOrDefaultAsync(c => c.Id == id && c.AgencyId == agencyId);
            
            if (contact != null)
            {
                _context.AgencyPhonebooks.Remove(contact);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kayýt silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

