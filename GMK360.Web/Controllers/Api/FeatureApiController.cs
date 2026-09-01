using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using Microsoft.AspNetCore.Authorization;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FeatureApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FeatureApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class FeatureCreateDto
        {
            public string Name { get; set; }
            public string Type { get; set; } // "IN" veya "OUT"
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] FeatureCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Type))
            {
                return BadRequest("Özellik adı ve tipi zorunludur.");
            }

            var categoryCode = dto.Type == "IN" ? "PROPERTY_FEATURE_IN" : 
                               dto.Type == "OUT" ? "PROPERTY_FEATURE_OUT" : "PROPERTY_FEATURE_KV";
            
            var category = await _context.DefinitionCategories.FirstOrDefaultAsync(c => c.SystemCode == categoryCode);
            if (category == null)
            {
                // Kategori yoksa otomatik oluştur (Migration gerekmesin diye tam bağımsızlık)
                category = new DefinitionCategory
                {
                    Name = dto.Type == "IN" ? "İç Özellikler" : dto.Type == "OUT" ? "Dış Özellikler" : "Ekstra Değerli Özellikler",
                    SystemCode = categoryCode
                };
                _context.DefinitionCategories.Add(category);
                await _context.SaveChangesAsync();
            }

            // Aynı isimde var mı kontrolü
            var existing = await _context.DefinitionValues.FirstOrDefaultAsync(v => v.CategoryId == category.Id && v.Name.ToLower() == dto.Name.ToLower());
            if (existing != null)
            {
                return Ok(new { id = existing.Id, name = existing.Name }); // Varsa direkt onu dön
            }

            var maxOrder = await _context.DefinitionValues.Where(v => v.CategoryId == category.Id).Select(v => (int?)v.Order).MaxAsync() ?? 0;

            var newValue = new DefinitionValue
            {
                CategoryId = category.Id,
                Name = dto.Name,
                Order = maxOrder + 1
            };

            _context.DefinitionValues.Add(newValue);
            await _context.SaveChangesAsync();

            return Ok(new { id = newValue.Id, name = newValue.Name });
        }
    }
}
