using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using GMK360.Web.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace GMK360.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowExtension")]
    public class ScraperApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ScraperApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("SaveProperty")]
        public async Task<IActionResult> SaveProperty([FromBody] ScrapedPropertyModel model)
        {
            if (model == null)
                return BadRequest("Geçersiz veri.");

            var entity = new ScrapedProperty
            {
                Title = model.Title,
                PriceText = model.PriceText,
                GrossAreaText = model.GrossAreaText,
                NetAreaText = model.NetAreaText,
                RoomCount = model.RoomCount,
                BuildingAge = model.BuildingAge,
                FloorNumberText = model.FloorNumberText,
                Description = model.Description,
                Url = model.Url,
                Platform = model.Platform,
                CreatedAt = DateTime.UtcNow,
                IsApproved = false
            };

            _context.ScrapedProperties.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "İlan başarıyla havuza alındı.", data = entity });
        }
    }
}
