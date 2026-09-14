using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Identity;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class AgencyStaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AgencyStaffController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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

        [HttpGet]
        public async Task<IActionResult> Invite()
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Invite(string email, string firstName, string lastName, string role, string phoneNumber)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(role))
            {
                ModelState.AddModelError("", "L�tfen gerekli t�m alanlar� doldurun.");
                return View();
            }

            // 1. Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                // Create New User
                existingUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phoneNumber,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };
                var result = await _userManager.CreateAsync(existingUser, "TempPass123!*"); // In real app, send invite email
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View();
                }
                
                // Assign Identity Role (InsaatFirmasi - required to access this dashboard)
                await _userManager.AddToRoleAsync(existingUser, "InsaatFirmasi");
            }

            // 2. Add to AgencyConsultants with specific Title/Role
            var existingConsultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == existingUser.Id && c.AgencyId == agencyId);
            if (existingConsultant == null)
            {
                var newConsultant = new GMK360.Core.Entities.AgencyConsultant
                {
                    AgencyId = agencyId.Value,
                    UserId = existingUser.Id,
                    Role = Enum.Parse<GMK360.Core.Entities.AgencyRole>(role),
                    StartDate = DateTime.UtcNow,
                    IsActive = true
                };
                _context.AgencyConsultants.Add(newConsultant);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Kullan�c� ba�ar�yla eklendi ve yetkilendirildi.";
            return RedirectToAction(nameof(Index));
        }

                [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var staff = await _context.AgencyConsultants
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id && a.AgencyId == agencyId);

            if (staff == null) return NotFound();

            // Load Payrolls and Advances
            var payrolls = await _context.AgencyStaffPayrolls
                .Where(p => p.AgencyConsultantId == id && !p.IsDeleted)
                .OrderByDescending(p => p.Period)
                .ToListAsync();

            var advances = await _context.AgencyStaffAdvances
                .Where(a => a.AgencyConsultantId == id && !a.IsDeleted)
                .OrderByDescending(a => a.RequestDate)
                .ToListAsync();

            ViewBag.Payrolls = payrolls;
            ViewBag.Advances = advances;

            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFinance(int id, decimal monthlySalary, string iban, string identityNumber)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var staff = await _context.AgencyConsultants.FirstOrDefaultAsync(a => a.Id == id && a.AgencyId == agencyId);
            if (staff == null) return NotFound();

            staff.MonthlySalary = monthlySalary;
            staff.IBAN = iban;
            staff.IdentityNumber = identityNumber;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Finans bilgileri güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Index()
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var staff = await _context.AgencyConsultants
                .Include(c => c.User)
                .Where(c => c.AgencyId == agencyId)
                .ToListAsync();

            return View(staff);
        }
    }
}



