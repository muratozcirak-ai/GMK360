using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestRates()
        {
            var latestRate = await _context.ExchangeRates
                .OrderByDescending(r => r.Date)
                .FirstOrDefaultAsync();

            if (latestRate == null)
            {
                return Ok(new { usd = 33.20m, eur = 35.50m, gold = 2500m }); // Mock fallback
            }

            return Ok(new
            {
                usd = latestRate.UsdRate,
                eur = latestRate.EurRate,
                gold = 2500m // TCMB'den altın çekmiyorsak şimdilik mock veya statik
            });
        }
    }
}
