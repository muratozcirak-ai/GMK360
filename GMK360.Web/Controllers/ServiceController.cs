using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Web.Models;
using GMK360.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace GMK360.Web.Controllers
{
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Search(string type = "All", int? cityId = null, int? districtId = null, string q = null, bool verifiedOnly = false)
        {
            var experts = new List<ExpertSearchViewModel>();

            // 1. USTALAR VE FİRMALAR (Service Providers)
            if (type == "All" || type == "Usta" || type == "Firma")
            {
                var spQuery = _context.ServiceProviders
                    .Include(sp => sp.Areas).ThenInclude(a => a.City)
                    .Include(sp => sp.Areas).ThenInclude(a => a.District)
                    .Include(sp => sp.Services).ThenInclude(s => s.ServiceCategory)
                    .AsQueryable();

                if (verifiedOnly) spQuery = spQuery.Where(x => x.IsVerified);
                if (type == "Usta") spQuery = spQuery.Where(x => !x.IsCorporate);
                if (type == "Firma") spQuery = spQuery.Where(x => x.IsCorporate);
                if (cityId.HasValue) spQuery = spQuery.Where(x => x.Areas.Any(a => a.CityId == cityId.Value));
                if (districtId.HasValue) spQuery = spQuery.Where(x => x.Areas.Any(a => a.DistrictId == districtId.Value));
                if (!string.IsNullOrEmpty(q)) spQuery = spQuery.Where(x => x.BusinessName.Contains(q));

                var sps = await spQuery.ToListAsync();

                foreach (var sp in sps)
                {
                    var catList = sp.Services?.Select(s => s.ServiceCategory.Name).ToArray() ?? new string[0];
                    var location = sp.Areas?.FirstOrDefault() != null 
                                    ? $"{sp.Areas.First().District?.Name}, {sp.Areas.First().City?.Name}" 
                                    : "Genel";

                    experts.Add(new ExpertSearchViewModel
                    {
                        Id = sp.Id.ToString(),
                        Type = sp.IsCorporate ? "Firma" : "Usta",
                        Name = sp.BusinessName,
                        AvatarUrl = "/images/default-avatar.png", // Varsayılan veya sp.LogoUrl
                        AverageRating = sp.AverageRating,
                        ReviewCount = sp.TotalRatings,
                        IsVerified = sp.IsVerified,
                        Location = location,
                        Categories = catList,
                        Phone = "", // Phone is not available on ServiceProvider
                        ProfileUrl = $"/Usta/Detail/{sp.Id}"
                    });
                }
            }

            // 2. EMLAK DANIŞMANLARI (Agency Consultants)
            if (type == "All" || type == "Emlak Danışmanı")
            {
                var agQuery = _context.AgencyConsultants
                    .Include(ac => ac.User)
                    .Include(ac => ac.Agency)
                    .Where(ac => ac.IsActive)
                    .AsQueryable();

                // Not: Emlak Danışmanları için lokasyon filtrelemesi daha karmaşık olabilir (Ofisin veya Kullanıcının konumu)
                // Şimdilik User name ve search query üzerinden filtreleme yapalım
                if (!string.IsNullOrEmpty(q)) 
                {
                    agQuery = agQuery.Where(x => (x.User.FirstName + " " + x.User.LastName).Contains(q) || x.Agency.CompanyName.Contains(q));
                }

                if (verifiedOnly)
                {
                    agQuery = agQuery.Where(x => x.User.IsEDevletVerified); // Varsayımsal
                }

                var agents = await agQuery.ToListAsync();

                foreach (var ag in agents)
                {
                    experts.Add(new ExpertSearchViewModel
                    {
                        Id = ag.UserId, // String ID
                        Type = "Emlak Danışmanı",
                        Name = $"{ag.User.FirstName} {ag.User.LastName}",
                        AvatarUrl = string.IsNullOrEmpty(ag.User.ProfileImageUrl) ? "/images/default-avatar.png" : ag.User.ProfileImageUrl,
                        AverageRating = 4.5, // Varsayımsal (AgentRating eklendiğinde değişecek)
                        ReviewCount = 12, // Varsayımsal
                        IsVerified = ag.User.IsEDevletVerified,
                        Location = ag.Agency?.Address ?? "Genel", // Varsayımsal Ofis Konumu
                        Categories = new string[] { ag.Agency?.CompanyName ?? "Bağımsız", "Gayrimenkul Satış", "Kiralama" },
                        Phone = ag.User.PhoneNumber,
                        ProfileUrl = $"/Agent/Detail/{ag.UserId}"
                    });
                }
            }

            // Sıralama Mantığı: Onaylılar üstte, sonra puana göre azalan
            var sortedExperts = experts
                .OrderByDescending(e => e.IsVerified)
                .ThenByDescending(e => e.AverageRating)
                .ThenByDescending(e => e.ReviewCount)
                .ToList();

            ViewBag.CurrentType = type;
            ViewBag.CurrentCity = cityId;
            ViewBag.CurrentDistrict = districtId;
            ViewBag.CurrentQ = q;
            ViewBag.VerifiedOnly = verifiedOnly;

            // Şehirler Listesi (Filtre için)
            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();

            return View(sortedExperts);
        }
    }
}
