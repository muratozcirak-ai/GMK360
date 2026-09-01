using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class LocationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var countries = await _context.Countries.ToListAsync();
            return View("Countries", countries);
        }

        [HttpPost]
        public async Task<IActionResult> AddCountry(string name, string code)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _context.Countries.Add(new Country { Name = name, Code = code });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Cities(int countryId)
        {
            ViewBag.Country = await _context.Countries.FindAsync(countryId);
            var cities = await _context.Cities.Where(c => c.CountryId == countryId).ToListAsync();
            return View(cities);
        }

        [HttpPost]
        public async Task<IActionResult> AddCity(int countryId, string name, string plateCode)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _context.Cities.Add(new City { CountryId = countryId, Name = name, PlateCode = plateCode });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Cities), new { countryId });
        }

        public async Task<IActionResult> Districts(int cityId)
        {
            ViewBag.City = await _context.Cities.FindAsync(cityId);
            var districts = await _context.Districts.Where(d => d.CityId == cityId).ToListAsync();
            return View(districts);
        }

        [HttpPost]
        public async Task<IActionResult> AddDistrict(int cityId, string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _context.Districts.Add(new District { CityId = cityId, Name = name });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Districts), new { cityId });
        }

        public async Task<IActionResult> Neighborhoods(int districtId)
        {
            ViewBag.District = await _context.Districts.Include(d => d.City).FirstOrDefaultAsync(d => d.Id == districtId);
            var neighborhoods = await _context.Neighborhoods.Where(n => n.DistrictId == districtId).ToListAsync();
            return View(neighborhoods);
        }

        [HttpPost]
        public async Task<IActionResult> AddNeighborhood(int districtId, string name, string zipCode)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _context.Neighborhoods.Add(new Neighborhood { DistrictId = districtId, Name = name, ZipCode = zipCode });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Neighborhoods), new { districtId });
        }

        public async Task<IActionResult> ComplexesAndStreets(int neighborhoodId)
        {
            ViewBag.Neighborhood = await _context.Neighborhoods.Include(n => n.District).ThenInclude(d => d.City).FirstOrDefaultAsync(n => n.Id == neighborhoodId);
            ViewBag.Complexes = await _context.Complexes.Where(c => c.NeighborhoodId == neighborhoodId).ToListAsync();
            var streets = await _context.Streets.Where(s => s.NeighborhoodId == neighborhoodId).ToListAsync();
            return View(streets);
        }

        [HttpPost]
        public async Task<IActionResult> AddStreet(int neighborhoodId, string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _context.Streets.Add(new Street { NeighborhoodId = neighborhoodId, Name = name });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ComplexesAndStreets), new { neighborhoodId });
        }

        [HttpPost]
        public async Task<IActionResult> AddComplex(int neighborhoodId, string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _context.Complexes.Add(new Complex { NeighborhoodId = neighborhoodId, Name = name });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ComplexesAndStreets), new { neighborhoodId });
        }
    }
}
