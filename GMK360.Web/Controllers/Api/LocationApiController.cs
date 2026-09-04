using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocationApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("cities")]
        public async Task<IActionResult> GetCities()
        {
            // Assuming Turkey is countryId = 1, or just fetch all
            var cities = await _context.Cities
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name, c.PlateCode })
                .ToListAsync();
            return Ok(cities);
        }

        [HttpGet("districts/{cityId}")]
        public async Task<IActionResult> GetDistricts(int cityId)
        {
            var districts = await _context.Districts
                .Where(d => d.CityId == cityId)
                .OrderBy(d => d.Name)
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();
            return Ok(districts);
        }

        [HttpGet("neighborhoods/{districtId}")]
        public async Task<IActionResult> GetNeighborhoods(int districtId)
        {
            var data = await _context.Neighborhoods
                .Where(n => n.DistrictId == districtId)
                .OrderBy(n => n.Name)
                .Select(n => new { id = n.Id, name = n.Name })
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("complexes/street/{streetId}")]
        public async Task<IActionResult> GetComplexesByStreet(int streetId)
        {
            var complexes = await _context.Buildings
                .Where(b => b.StreetId == streetId && b.HasBlock && b.IsApproved)
                .GroupBy(b => b.Name)
                .Select(g => new { name = g.Key })
                .OrderBy(c => c.name)
                .ToListAsync();

            return Ok(complexes);
        }

        [HttpGet("streets/{neighborhoodId}")]
        public async Task<IActionResult> GetStreets(int neighborhoodId)
        {
            var streets = await _context.Streets
                .Where(s => s.NeighborhoodId == neighborhoodId)
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();
            return Ok(streets);
        }

        [HttpGet("buildings/{streetId}")]
        public async Task<IActionResult> GetBuildingsByStreet(int streetId)
        {
            // Return existing buildings on this street to prevent duplicates
            var buildings = await _context.Buildings
                .Where(b => b.StreetId == streetId && !b.HasBlock && b.IsApproved)
                .OrderBy(b => b.Name)
                .Select(b => new { b.Id, b.Name, b.BlockName, b.BuildingNumber })
                .ToListAsync();
            return Ok(buildings);
        }

        [HttpGet("complexes/search")]
        public async Task<IActionResult> SearchComplexes(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 3)
                return Ok(new List<object>());

            var complexes = await _context.Buildings
                .Include(b => b.City)
                .Include(b => b.District)
                .Include(b => b.Neighborhood)
                .Where(b => b.HasBlock && b.IsApproved && b.Name.Contains(q))
                .GroupBy(b => new { b.Name, b.CityId, b.DistrictId, b.NeighborhoodId, b.StreetId, CityName = b.City.Name, DistrictName = b.District.Name, NeighborhoodName = b.Neighborhood.Name })
                .Select(g => new 
                {
                    id = g.Key.Name, // We use Name as ID since it's the complex name
                    text = $"{g.Key.Name} ({g.Key.NeighborhoodName}, {g.Key.DistrictName}/{g.Key.CityName})",
                    name = g.Key.Name,
                    cityId = g.Key.CityId,
                    districtId = g.Key.DistrictId,
                    neighborhoodId = g.Key.NeighborhoodId,
                    streetId = g.Key.StreetId,
                    cityName = g.Key.CityName,
                    districtName = g.Key.DistrictName,
                    neighborhoodName = g.Key.NeighborhoodName
                })
                .Take(20)
                .ToListAsync();

            return Ok(complexes);
        }
    }
}
