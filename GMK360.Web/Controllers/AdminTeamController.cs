using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities.Identity;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminTeamController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AdminTeamController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // --- Ekip Listesi ---
        public async Task<IActionResult> Index()
        {
            // Sadece Sistem Rollerine Sahip KullanÄ±cÄ±lar (Sakin ve Usta hariÃ§)
            var allUsers = await _userManager.Users.ToListAsync();
            var staffUsers = new System.Collections.Generic.List<ApplicationUser>();
            var userRolesMap = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IList<string>>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                // EÄŸer "SuperAdmin", "Muhasebe", "Destek", "TeknikServis", "HalklaIliskiler", "Pazarlama" rollerinden birine sahipse
                var isStaff = roles.Any(r => new[] { "SuperAdmin", "Muhasebe", "Destek", "TeknikServis", "HalklaIliskiler", "Pazarlama" }.Contains(r));
                
                if (isStaff)
                {
                    staffUsers.Add(user);
                    userRolesMap[user.Id] = roles;
                }
            }

            ViewBag.UserRolesMap = userRolesMap;
            return View(staffUsers);
        }

        // --- Personel Ekle (GET) ---
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Sadece yÃ¶netici rollerini seÃ§ilebilir yapalÄ±m
            var roles = await _roleManager.Roles
                .Where(r => new[] { "SuperAdmin", "Muhasebe", "Destek", "TeknikServis", "HalklaIliskiler", "Pazarlama", "BolgeSorumlusu" }.Contains(r.Name))
                .ToListAsync();
                
            ViewBag.Roles = roles;
            return View();
        }

        // --- Personel Ekle (POST) ---
        [HttpPost]
        public async Task<IActionResult> Create(string firstName, string lastName, string email, string password, string[] selectedRoles)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "E-posta ve ÅŸifre zorunludur.");
                ViewBag.Roles = await _roleManager.Roles.Where(r => new[] { "SuperAdmin", "Muhasebe", "Destek", "TeknikServis", "HalklaIliskiler", "Pazarlama", "BolgeSorumlusu" }.Contains(r.Name)).ToListAsync();
                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                UserType = UserType.Corporate // Personel olarak iÅŸaretleyebiliriz
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                if (selectedRoles != null && selectedRoles.Any())
                {
                    await _userManager.AddToRolesAsync(user, selectedRoles);
                }
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.Roles = await _roleManager.Roles.Where(r => new[] { "SuperAdmin", "Muhasebe", "Destek", "TeknikServis", "HalklaIliskiler", "Pazarlama", "BolgeSorumlusu" }.Contains(r.Name)).ToListAsync();
            return View();
        }

        // --- Personel AskÄ±ya Al / Durum DeÄŸiÅŸtir ---
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            
            // GerÃ§ek projede IsActive gibi bir property kullanÄ±lÄ±r. 
            // Åimdilik LockoutEnd kullanarak pasif yapalÄ±m.
            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                user.LockoutEnd = null; // Aktif yap
            }
            else
            {
                user.LockoutEnd = DateTimeOffset.MaxValue; // SÃ¼resiz askÄ±ya al
            }
            
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}
