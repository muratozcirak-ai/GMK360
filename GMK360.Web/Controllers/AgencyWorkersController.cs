using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class AgencyWorkersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AgencyWorkersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Assuming agency ID 1 for now (multi-tenant logic handled normally)
            var workers = await _context.Set<AgencyWorker>()
                .Include(w => w.User)
                .Where(w => w.IsActive)
                .ToListAsync();
                
            return View(workers);
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(string firstName, string lastName, string phoneNumber, string profession, decimal dailyWage)
        {
            try
            {
                // Altın kural: Mavi yakayı da ana sisteme kaydediyoruz
                var user = await _userManager.FindByNameAsync(phoneNumber);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = phoneNumber,
                        PhoneNumber = phoneNumber,
                        FirstName = firstName,
                        LastName = lastName,
                        UserType = UserType.ServiceProvider
                        
                    };
                    await _userManager.CreateAsync(user, "MaviYaka123!");
                }

                var worker = new AgencyWorker
                {
                    AgencyId = 1,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phoneNumber,
                    Profession = profession,
                    DefaultDailyWage = dailyWage,
                    NetDailyWage = dailyWage,
                    UserId = user.Id
                };

                _context.Add(worker);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Mavi Yaka Personel (Usta/İşçi) sisteme ve global havuza başarıyla eklendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata: " + ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}