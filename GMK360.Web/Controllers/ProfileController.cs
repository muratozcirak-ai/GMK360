using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using GMK360.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GMK360.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Dinamik Route: /usta/{slug}, /danisman/{slug}, /kurumsal/{slug}
        
        [Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            
            return View(user);
        }

[HttpGet("{userTypeSlug:regex(^(usta|danisman|kurumsal)$)}/{profileSlug}")]
        public async Task<IActionResult> Detail(string userTypeSlug, string profileSlug)
        {
            // Kullanıcıyı Slug'dan bul (Büyük-küçük harf duyarsız arama)
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.ProfileSlug == profileSlug);

            if (user == null)
            {
                return NotFound("Profil bulunamadı.");
            }

            // Gelen URL type ile kullanıcının gerçek type'ı eşleşiyor mu kontrolü
            if ((userTypeSlug == "usta" && user.UserType != UserType.ServiceProvider) ||
                (userTypeSlug == "danisman" && user.UserType != UserType.Consultant) ||
                (userTypeSlug == "kurumsal" && user.UserType != UserType.Corporate))
            {
                return NotFound("Geçersiz profil url eşleşmesi.");
            }

            // Bireysel profil gizliliği (KVKK)
            if (user.UserType == UserType.Individual || user.UserType == UserType.PropertyOwner)
            {
                // TODO: Gelecekte yetkili mülk sahipleri vb. için açılabilir
                return Unauthorized("Bu kullanıcı tipi için açık profil sayfası bulunmamaktadır.");
            }

            var model = new PublicProfileViewModel
            {
                ProfileUserId = user.Id,
                DisplayName = !string.IsNullOrEmpty(user.DisplayName) ? user.DisplayName : $"{user.FirstName} {user.LastName}",
                UserType = user.UserType,
                ProfileImageUrl = user.ProfileImageUrl,
                Bio = "Platformumuza hoş geldiniz. Kullanıcı profil açıklaması eklenecektir.", // TODO: ApplicationUser'a Bio eklenebilir.
                IsVerifiedUser = user.IsEDevletVerified,
                ReliabilityScore = 100 // Varsayılan
            };

            if (user.UserType == UserType.ServiceProvider)
            {
                var provider = await _context.ServiceProviders
                    .Include(s => s.Services)
                        .ThenInclude(ss => ss.ServiceCategory)
                    .Include(s => s.Ratings)
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);

                if (provider != null)
                {
                    model.IsCorporateVerified = provider.IsCorporate; // veya IsVerified
                    model.IsEmergencyModeActive = provider.IsEmergencyModeActive;
                    model.ReliabilityScore = provider.ReliabilityScore;
                    model.ProviderServices = provider.Services.ToList();
                    model.AverageRating = provider.AverageRating;
                    model.TotalRatings = provider.TotalRatings;
                    model.BusinessName = provider.BusinessName;
                }
            }
            else if (user.UserType == UserType.Consultant || user.UserType == UserType.Corporate)
            {
                // Danışman / Kurumsal ilanlarını getir
                var activeListings = await _context.Properties
                    .Where(p => p.UserId == user.Id && p.State == ListingState.Active)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(12)
                    .ToListAsync();
                    
                model.ActiveListings = activeListings;
                
                var corp = await _context.CorporateProfiles.FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id);
                if (corp != null)
                {
                    model.IsCorporateVerified = true; // Corp profile exists
                    model.BusinessName = corp.CompanyName;
                }
            }

            return View(model);
        }

        [HttpPost("Profile/AcceptMapConsent")]
        [Authorize]
        [IgnoreAntiforgeryToken] // Layout'tan ajax ile gelirken token uyuşmazlığı olmaması için
        public async Task<IActionResult> AcceptMapConsent()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.HasMapConsent = true;
                user.MapConsentDate = System.DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                return Ok();
            }
            return Unauthorized();
        }

    }
}
