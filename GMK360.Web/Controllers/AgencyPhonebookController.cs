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

            var query = _context.AgencyPhonebooks.Where(p => p.AgencyId == agencyId && !p.IsDeleted);
            
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

            // Altın Kural: B2BNetworkConnections (Pazar Yeri Bağlantıları) üzerinden gelen firmaları da listeye dahil et
            var b2bConnections = await _context.B2BNetworkConnections
                .Include(c => c.B2bCompany)
                .Where(c => c.AgencyId == agencyId && c.IsActive && !c.IsDeleted)
                .ToListAsync();

            foreach (var conn in b2bConnections)
            {
                // B2bCompany nesnesini AgencyPhonebook nesnesi gibi (sanal olarak) View'a gönderiyoruz
                if (conn.B2bCompany != null)
                {
                    byte contactType = 2; // Default: Taşeron Firma
                    if (conn.B2bCompany.IsSupplier) contactType = 3; // Tedarikçi
                    if (conn.B2bCompany.IsSubcontractor) contactType = 1; // Usta
                    
                    // Sadece seçili filtreye uygun olanları ekle
                    if (!type.HasValue || type.Value == contactType || type.Value == 0)
                    {
                        contacts.Add(new AgencyPhonebook
                        {
                            Id = -conn.B2bCompany.Id, // Negatif ID veriyoruz ki Pazar Yeri kaydı olduğu anlaşılsın
                            AgencyId = agencyId.Value,
                            Name = conn.B2bCompany.Name + " (B2B)",
                            ContactType = contactType,
                            PhoneNumber = "B2B Pazar Yeri",
                            Notes = "Pazar Yeri (B2B) üzerinden bağlandı. " + conn.ConnectionNotes
                        });
                    }
                }
            }

            return View(contacts.OrderBy(c => c.Name).ToList());
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
                TempData["SuccessMessage"] = "Kişi/Firma rehbere eklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Lütfen zorunlu alanları doldurun.";
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
            // Şimdilik sadece toast mesajı vereceğiz
            
            TempData["SuccessMessage"] = $"{contact.Name} adlı kişiye sisteme katılım daveti gönderildi.";
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
                TempData["SuccessMessage"] = "Kayıt silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

