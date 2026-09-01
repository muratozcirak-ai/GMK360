using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminLocationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<GMK360.Core.Entities.Identity.ApplicationUser> _userManager;

        public AdminLocationController(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<GMK360.Core.Entities.Identity.ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- DASHBOARD ---
        public async Task<IActionResult> Index()
        {
            ViewBag.CityCount = await _context.Cities.CountAsync();
            ViewBag.DistrictCount = await _context.Districts.CountAsync();
            ViewBag.NeighborhoodCount = await _context.Neighborhoods.CountAsync();
            ViewBag.StreetCount = await _context.Streets.CountAsync();
            ViewBag.BuildingCount = await _context.Buildings.CountAsync();
            
            return View();
        }

        // --- API ENDPOINTS FOR CASCADING ---
        [HttpGet]
        public async Task<IActionResult> GetDistricts(int cityId)
        {
            var districts = await _context.Districts
                .Where(d => d.CityId == cityId)
                .OrderBy(d => d.Name)
                .Select(d => new { id = d.Id, name = d.Name })
                .ToListAsync();
            return Json(districts);
        }

        [HttpGet]
        public async Task<IActionResult> GetNeighborhoods(int districtId)
        {
            var neighborhoods = await _context.Neighborhoods
                .Where(n => n.DistrictId == districtId)
                .OrderBy(n => n.Name)
                .Select(n => new { id = n.Id, name = n.Name })
                .ToListAsync();
            return Json(neighborhoods);
        }

        // --- CITIES ---
        public async Task<IActionResult> Cities()
        {
            var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            return View(cities);
        }

        [HttpPost]
        public async Task<IActionResult> AddCity(string name, string plateCode)
        {
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(plateCode))
            {
                var city = new City { Name = name, PlateCode = plateCode, CountryId = 1, CreatedAt = DateTime.UtcNow };
                _context.Cities.Add(city);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Cities));
        }

        [HttpPost]
        public async Task<IActionResult> EditCity(int id, string name)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city != null && !string.IsNullOrWhiteSpace(name))
            {
                city.Name = name;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Cities));
        }

        // --- DISTRICTS ---
        public async Task<IActionResult> Districts(int? cityId)
        {
            var query = _context.Districts.Include(d => d.City).AsQueryable();
            if (cityId.HasValue)
                query = query.Where(d => d.CityId == cityId.Value);
                
            var districts = await query.OrderBy(d => d.City.Name).ThenBy(d => d.Name).ToListAsync();
            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            ViewBag.SelectedCityId = cityId;
            return View(districts);
        }

        [HttpPost]
        public async Task<IActionResult> AddDistrict(string name, int cityId)
        {
            if (!string.IsNullOrWhiteSpace(name) && cityId > 0)
            {
                var district = new District { Name = name, CityId = cityId, CreatedAt = DateTime.UtcNow };
                _context.Districts.Add(district);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Districts), new { cityId });
        }

        [HttpPost]
        public async Task<IActionResult> EditDistrict(int id, string name, int cityId)
        {
            var district = await _context.Districts.FindAsync(id);
            if (district != null && !string.IsNullOrWhiteSpace(name) && cityId > 0)
            {
                district.Name = name;
                district.CityId = cityId; // RELOCATION
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Districts), new { cityId = district?.CityId });
        }

        // --- NEIGHBORHOODS ---
        public async Task<IActionResult> Neighborhoods(int? cityId, int? districtId)
        {
            var query = _context.Neighborhoods.Include(n => n.District).ThenInclude(d => d.City).AsQueryable();
            
            if (districtId.HasValue)
                query = query.Where(n => n.DistrictId == districtId.Value);
            else if (cityId.HasValue)
                query = query.Where(n => n.District.CityId == cityId.Value);
                
            var neighborhoods = await query.OrderBy(n => n.District.City.Name).ThenBy(n => n.District.Name).ThenBy(n => n.Name).Take(500).ToListAsync();
            
            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            ViewBag.SelectedCityId = cityId;
            ViewBag.SelectedDistrictId = districtId;
            
            return View(neighborhoods);
        }

        [HttpPost]
        public async Task<IActionResult> AddNeighborhood(string name, int districtId)
        {
            if (!string.IsNullOrWhiteSpace(name) && districtId > 0)
            {
                var neighborhood = new Neighborhood { Name = name, DistrictId = districtId, CreatedAt = DateTime.UtcNow };
                _context.Neighborhoods.Add(neighborhood);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Neighborhoods), new { districtId });
        }

        [HttpPost]
        public async Task<IActionResult> EditNeighborhood(int id, string name, int districtId)
        {
            var neighborhood = await _context.Neighborhoods.FindAsync(id);
            if (neighborhood != null && !string.IsNullOrWhiteSpace(name) && districtId > 0)
            {
                neighborhood.Name = name;
                neighborhood.DistrictId = districtId; // RELOCATION
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Neighborhoods), new { districtId = neighborhood?.DistrictId });
        }

        // --- STREETS ---
        public async Task<IActionResult> Streets(int? cityId, int? districtId, int? neighborhoodId, string searchQuery, bool isAjax = false)
        {
            if (!isAjax)
            {
                ViewBag.Cities = await _context.Cities.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
                ViewBag.SelectedCityId = cityId;
                ViewBag.SelectedDistrictId = districtId;
                ViewBag.SelectedNeighborhoodId = neighborhoodId;
                ViewBag.SearchQuery = searchQuery;
            }

            // Performans Optimizasyonu: Filtre yoksa veritabanını yorma
            if (!cityId.HasValue && !districtId.HasValue && !neighborhoodId.HasValue && string.IsNullOrWhiteSpace(searchQuery))
            {
                if (isAjax) return Json(new List<object>());
                return View(new List<Street>());
            }

            var query = _context.Streets
                .Include(s => s.Neighborhood)
                .ThenInclude(n => n.District)
                .ThenInclude(d => d.City)
                .AsNoTracking() // 1.1 Milyon kayıt için kritik performans artışı
                .AsQueryable();

            if (neighborhoodId.HasValue)
                query = query.Where(s => s.NeighborhoodId == neighborhoodId.Value);
            else if (districtId.HasValue)
                query = query.Where(s => s.Neighborhood.DistrictId == districtId.Value);
            else if (cityId.HasValue)
                query = query.Where(s => s.Neighborhood.District.CityId == cityId.Value);

            if (!string.IsNullOrWhiteSpace(searchQuery))
                query = query.Where(s => s.Name.Contains(searchQuery));

            var streets = await query.OrderBy(s => s.Name).Take(500).ToListAsync();
            
            if (isAjax)
            {
                var data = streets.Select(s => new {
                    id = s.Id,
                    cityName = s.Neighborhood?.District?.City?.Name,
                    districtName = s.Neighborhood?.District?.Name,
                    neighborhoodName = s.Neighborhood?.Name,
                    name = s.Name,
                    isActive = s.IsActive,
                    cityId = s.Neighborhood?.District?.CityId,
                    districtId = s.Neighborhood?.DistrictId,
                    neighborhoodId = s.NeighborhoodId
                });
                return Json(data);
            }

            return View(streets);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStreetStatus(int id)
        {
            var street = await _context.Streets.FindAsync(id);
            if (street == null) return NotFound();
            
            street.IsActive = !street.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, isActive = street.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> BulkMoveStreets([FromBody] BulkMoveRequest request)
        {
            if (request == null || request.StreetIds == null || !request.StreetIds.Any() || request.TargetNeighborhoodId <= 0)
                return BadRequest("Geçersiz veri gönderildi.");

            var streets = await _context.Streets.Where(s => request.StreetIds.Contains(s.Id)).ToListAsync();
            foreach (var street in streets)
            {
                street.NeighborhoodId = request.TargetNeighborhoodId;
            }
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = $"{streets.Count} adet sokak başarıyla yeni mahalleye taşındı." });
        }


        [HttpPost]
        public async Task<IActionResult> AddStreet(string name, int neighborhoodId)
        {
            if (!string.IsNullOrWhiteSpace(name) && neighborhoodId > 0)
            {
                var street = new Street { Name = name, NeighborhoodId = neighborhoodId, CreatedAt = DateTime.UtcNow };
                _context.Streets.Add(street);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Streets), new { neighborhoodId });
        }

        [HttpPost]
        public async Task<IActionResult> EditStreet(int id, string name, int neighborhoodId)
        {
            var street = await _context.Streets.FindAsync(id);
            if(street != null && !string.IsNullOrWhiteSpace(name) && neighborhoodId > 0)
            {
                street.Name = name;
                street.NeighborhoodId = neighborhoodId; // RELOCATION
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Streets), new { neighborhoodId = street?.NeighborhoodId });
        }

        // --- BUILDINGS (Binalar ve Birleştirme/Merge) ---
        public async Task<IActionResult> Buildings(int? streetId, string searchQuery)
        {
            var query = _context.Buildings
                .Include(b => b.City)
                .Include(b => b.District)
                .Include(b => b.Neighborhood)
                .AsQueryable();

            if (streetId.HasValue)
                query = query.Where(b => b.StreetId == streetId.Value);

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(b => b.Name.Contains(searchQuery));

            var buildings = await query.OrderByDescending(b => b.Id).Take(100).ToListAsync();
            return View(buildings);
        }

        [HttpPost]
        public async Task<IActionResult> MergeBuildings(int sourceBuildingId, int targetBuildingId)
        {
            if (sourceBuildingId == targetBuildingId)
                return BadRequest("Aynı bina seçilemez.");

            var source = await _context.Buildings.FindAsync(sourceBuildingId);
            var target = await _context.Buildings.FindAsync(targetBuildingId);

            if (source == null || target == null)
                return NotFound("Bina bulunamadı.");

            var user = await _userManager.GetUserAsync(User);

            var properties = await _context.Properties.Where(p => p.BuildingId == sourceBuildingId).ToListAsync();
            foreach (var prop in properties)
            {
                prop.BuildingId = targetBuildingId;
            }

            var expenses = await _context.BuildingExpenses.Where(e => e.BuildingId == sourceBuildingId).ToListAsync();
            foreach (var exp in expenses) { exp.BuildingId = targetBuildingId; }

            var auditLog = new SystemAuditLog
            {
                ActionType = "MERGE_BUILDING",
                Description = $"Bina Birleştirme: '{source.Name}' (ID:{{source.Id}}) isimli bina, '{target.Name}' (ID:{{target.Id}}) ile birleştirildi ve silindi. Toplam {{properties.Count}} daire taşındı.",
                PerformedByUserId = user?.Id ?? "System",
                CreatedAt = DateTime.UtcNow
            };
            _context.SystemAuditLogs.Add(auditLog);

            _context.Buildings.Remove(source);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Binalar başarıyla birleştirildi ve hatalı kayıt silindi." });
        }

        // --- UNIVERSAL BULK MOVE ---
        public async Task<IActionResult> BulkMove()
        {
            ViewBag.Cities = await _context.Cities.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCityAjax([FromForm] string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("Şehir adı boş olamaz.");
            var city = new City { Name = name.Trim(), CreatedAt = DateTime.UtcNow };
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();
            return Ok(new { id = city.Id, name = city.Name });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDistrictAjax([FromForm] string name, [FromForm] int cityId)
        {
            if (string.IsNullOrWhiteSpace(name) || cityId <= 0) return BadRequest("Geçersiz veri.");
            var district = new District { Name = name.Trim(), CityId = cityId, CreatedAt = DateTime.UtcNow };
            _context.Districts.Add(district);
            await _context.SaveChangesAsync();
            return Ok(new { id = district.Id, name = district.Name });
        }

        [HttpPost]
        public async Task<IActionResult> CreateNeighborhoodAjax([FromForm] string name, [FromForm] int districtId)
        {
            if (string.IsNullOrWhiteSpace(name) || districtId <= 0) return BadRequest("Geçersiz veri.");
            var neighborhood = new Neighborhood { Name = name.Trim(), DistrictId = districtId, CreatedAt = DateTime.UtcNow };
            _context.Neighborhoods.Add(neighborhood);
            await _context.SaveChangesAsync();
            return Ok(new { id = neighborhood.Id, name = neighborhood.Name });
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteDistrictMove([FromBody] BulkMoveRequest req)
        {
            if (req.Ids == null || !req.Ids.Any() || req.TargetId <= 0) return BadRequest("Geçersiz veri.");
            var items = await _context.Districts.Where(d => req.Ids.Contains(d.Id)).ToListAsync();
            foreach (var item in items) item.CityId = req.TargetId;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = $"{items.Count} ilçe başarıyla yeni şehre taşındı." });
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteNeighborhoodMove([FromBody] BulkMoveRequest req)
        {
            if (req.Ids == null || !req.Ids.Any() || req.TargetId <= 0) return BadRequest("Geçersiz veri.");
            var items = await _context.Neighborhoods.Where(n => req.Ids.Contains(n.Id)).ToListAsync();
            foreach (var item in items) item.DistrictId = req.TargetId;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = $"{items.Count} mahalle başarıyla yeni ilçeye taşındı." });
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteStreetMove([FromBody] BulkMoveRequest req)
        {
            if (req.Ids == null || !req.Ids.Any() || req.TargetId <= 0) return BadRequest("Geçersiz veri.");
            var items = await _context.Streets.Where(s => req.Ids.Contains(s.Id)).ToListAsync();
            foreach (var item in items) item.NeighborhoodId = req.TargetId;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = $"{items.Count} sokak başarıyla yeni mahalleye taşındı." });
        }
    }

    public class BulkMoveRequest
    {
        public List<int>? StreetIds { get; set; } // for backward compatibility in Streets view
        public int TargetNeighborhoodId { get; set; } // for backward compatibility

        public List<int>? Ids { get; set; }
        public int TargetId { get; set; }
    }
}

