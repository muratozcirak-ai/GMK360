using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using GMK360.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SearchApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("GetListings")]
        public async Task<IActionResult> GetListings([FromBody] SearchFilterDto filter)
        {
            var query = _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Complex)
                    .ThenInclude(c => c.City)
                .Include(p => p.Complex)
                    .ThenInclude(c => c.District)
                .Include(p => p.Complex)
                    .ThenInclude(c => c.Neighborhood)
                .AsQueryable();

            // Sadece Aktif ilanlar
            query = query.Where(p => p.State == ListingState.Active);

            // Çoklu Seçim: Şehirler
            if (filter.CityIds != null && filter.CityIds.Any())
            {
                query = query.Where(p => p.Complex != null && filter.CityIds.Contains(p.Complex.CityId));
            }

            // Çoklu Seçim: İlçeler
            if (filter.DistrictIds != null && filter.DistrictIds.Any())
            {
                query = query.Where(p => p.Complex != null && filter.DistrictIds.Contains(p.Complex.DistrictId));
            }

            // Çoklu Seçim: Mahalleler
            if (filter.NeighborhoodIds != null && filter.NeighborhoodIds.Any())
            {
                query = query.Where(p => p.Complex != null && filter.NeighborhoodIds.Contains(p.Complex.NeighborhoodId));
            }

            // Çoklu Seçim: Kategoriler (SubType)
            if (filter.CategoryIds != null && filter.CategoryIds.Any())
            {
                query = query.Where(p => p.SubTypeId.HasValue && filter.CategoryIds.Contains(p.SubTypeId.Value));
            }

            // Fiyat Aralığı
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            }
            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            }

            // Omnibox Keyword Arama (Başlık, Açıklama veya Site Adı)
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.ToLower();
                query = query.Where(p => 
                    p.Title.ToLower().Contains(kw) || 
                    (p.Description != null && p.Description.ToLower().Contains(kw)) ||
                    (p.Complex != null && p.Complex.Name.ToLower().Contains(kw))
                );
            }

            // Toplam Kayıt Sayısı
            var totalRecords = await query.CountAsync();

            // Sayfalama (Pagination)
            var properties = await query
                .OrderByDescending(p => p.Id)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Price,
                    p.Currency,
                    p.RoomCount,
                    p.NetArea,
                    p.GrossArea,
                    CityName = p.Complex != null && p.Complex.City != null ? p.Complex.City.Name : "",
                    DistrictName = p.Complex != null && p.Complex.District != null ? p.Complex.District.Name : "",
                    ThumbnailUrl = p.Images.Any(i => i.IsCover) 
                                    ? p.Images.FirstOrDefault(i => i.IsCover).ImageUrl 
                                    : (p.Images.Any() ? p.Images.FirstOrDefault().ImageUrl : "/images/no-image.jpg")
                })
                .ToListAsync();

            return Ok(new
            {
                Data = properties,
                TotalRecords = totalRecords,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            });
        }
    }
}
