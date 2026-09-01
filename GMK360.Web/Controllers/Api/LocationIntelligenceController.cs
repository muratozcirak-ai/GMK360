using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationIntelligenceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocationIntelligenceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetNearbyPlaces")]
        public async Task<IActionResult> GetNearbyPlaces(int neighborhoodId)
        {
            var places = await _context.NeighborhoodPOIs
                .Where(p => p.NeighborhoodId == neighborhoodId)
                .OrderBy(p => p.DistanceInMeters)
                .Select(p => new
                {
                    p.Id,
                    p.PoiName,
                    Category = p.PoiCategory.ToString(), // Enum'ı string olarak döndürür
                    p.SubCategory,
                    p.DistanceInMeters
                })
                .ToListAsync();

            return Ok(places);
        }

        [HttpGet("GetLocalProfessionals")]
        public async Task<IActionResult> GetLocalProfessionals(int neighborhoodId)
        {
            var professionals = await _context.LocalProfessionals
                .Where(p => p.NeighborhoodId == neighborhoodId)
                .OrderByDescending(p => p.IsPremium) // Premium ustalar en üstte çıkar
                .ThenBy(p => p.ProfessionType)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.ProfessionType,
                    p.PhoneNumber,
                    p.IsPremium
                })
                .ToListAsync();

            return Ok(professionals);
        }

        [HttpGet("GetNeighboringNeighborhoods")]
        public async Task<IActionResult> GetNeighboringNeighborhoods(int neighborhoodId)
        {
            var neighbors = await _context.NeighboringAreas
                .Include(na => na.NeighborNeighborhood)
                .Where(na => na.BaseNeighborhoodId == neighborhoodId)
                .Select(na => new
                {
                    NeighborId = na.NeighborNeighborhoodId,
                    NeighborName = na.NeighborNeighborhood.Name,
                    DistanceInKm = na.DistanceInKilometers
                })
                .ToListAsync();

            return Ok(neighbors);
        }
    }
}
