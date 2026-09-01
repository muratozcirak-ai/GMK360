using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Cities")]
        public async Task<IActionResult> GetCities()
        {
            var cities = await _context.Cities
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            return Ok(cities);
        }

        [HttpGet("Districts/{cityId}")]
        public async Task<IActionResult> GetDistricts(int cityId)
        {
            var districts = await _context.Districts
                .Where(d => d.CityId == cityId)
                .OrderBy(d => d.RegionName)
                .ThenBy(d => d.Name)
                .Select(d => new { d.Id, d.Name, d.RegionName })
                .ToListAsync();
            return Ok(districts);
        }

        [HttpGet("Neighborhoods/{districtId}")]
        public async Task<IActionResult> GetNeighborhoods(int districtId)
        {
            var neighborhoods = await _context.Neighborhoods
                .Where(n => n.DistrictId == districtId)
                .OrderBy(n => n.Name)
                .Select(n => new { n.Id, n.Name, n.ZipCode })
                .ToListAsync();
            return Ok(neighborhoods);
        }

        [HttpGet("Streets/{neighborhoodId}")]
        public async Task<IActionResult> GetStreets(int neighborhoodId)
        {
            var streets = await _context.Streets
                .Where(s => s.NeighborhoodId == neighborhoodId)
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();
            return Ok(streets);
        }

        [HttpGet("POIs/{neighborhoodId}")]
        public async Task<IActionResult> GetPOIs(int neighborhoodId)
        {
            var pois = await _context.NeighborhoodPOIs
                .Where(p => p.NeighborhoodId == neighborhoodId)
                .OrderBy(p => p.PoiCategory)
                .ThenBy(p => p.DistanceInMeters)
                .Select(p => new {
                    p.Id,
                    p.PoiName,
                    p.PoiCategory,
                    CategoryName = p.PoiCategory.ToString(),
                    p.DistanceInMeters
                })
                .ToListAsync();
                
            return Ok(pois);
        }
    }
}
