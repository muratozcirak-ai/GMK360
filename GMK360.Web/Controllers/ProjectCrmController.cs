using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Entities.Identity;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class ProjectCrmController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectCrmController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var contacts = await _context.CrmContacts
                .Where(c => c.ApplicationUserId == userId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(contacts);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string FirstName, string LastName, string PhoneNumber, string ContactType, string Notes)
        {
            var userId = _userManager.GetUserId(User);
            
            var contact = new CrmContact
            {
                ApplicationUserId = userId,
                FirstName = FirstName,
                LastName = LastName,
                PhoneNumber = PhoneNumber,
                ContactType = ContactType, // Potansiyel Alıcı, Daire Sahibi vb.
                Notes = Notes
            };

            _context.CrmContacts.Add(contact);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Müşteri / Ziyaretçi CRM kaydı başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }
    }
}