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
            // Sadece Sistem Rollerine Sahip Kullanıcılar (Sakin ve Usta hariç)
            var allUsers = await _userManager.Users.ToListAsync();
            var staffUsers = new System.Collections.Generic.List<ApplicationUser>();
            var userRolesMap = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IList<string>>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                // Eğer "SuperAdmin", "Muhasebe", "Destek", "TeknikServis", "HalklaIliskiler", "Pazarlama" rollerinden birine sahipse
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
            // Sadece yönetici rollerini seçilebilir yapalım
            var roles = await _roleManager.Roles
                .Where(r => r.Name != "Sakin" && r.Name != "BinaYoneticisi" && r.Name != "Usta")
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
                ModelState.AddModelError("", "E-posta ve şifre zorunludur.");
                ViewBag.Roles = await _roleManager.Roles.Where(r => r.Name != "Sakin" && r.Name != "BinaYoneticisi" && r.Name != "Usta").ToListAsync();
                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                UserType = UserType.Corporate // Personel olarak işaretleyebiliriz
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

            ViewBag.Roles = await _roleManager.Roles.Where(r => r.Name != "Sakin" && r.Name != "BinaYoneticisi" && r.Name != "Usta").ToListAsync();
            return View();
        }

        // --- Personel Askıya Al / Durum Değiştir ---
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            
            // Gerçek projede IsActive gibi bir property kullanılır. 
            // Şimdilik LockoutEnd kullanarak pasif yapalım.
            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                user.LockoutEnd = null; // Aktif yap
            }
            else
            {
                user.LockoutEnd = DateTimeOffset.MaxValue; // Süresiz askıya al
            }
            
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}
