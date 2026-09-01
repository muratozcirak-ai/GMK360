using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;
using GMK360.Data.Contexts;

namespace GMK360.Data.Repositories
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly ApplicationDbContext _context;

        public PropertyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Geo-IP ve Konum tabanlı ana sayfa için
        public async Task<List<Property>> GetPropertiesByCityAsync(string city)
        {
            return await _context.Properties
                .Include(p => p.Building)
                .ThenInclude(b => b.City)
                .Where(p => p.Building.City.Name == city && !p.IsDeleted)
                .OrderByDescending(p => p.IsBoosted) // Dopingliler en üste!
                .ThenByDescending(p => p.CreatedAt) // Sonra en yeniler
                .ToListAsync();
        }

        // Dopingli/Sponsorlu ilanları getiren algoritma
        public async Task<List<Property>> GetFeaturedPropertiesAsync()
        {
            return await _context.Properties
                .Include(p => p.Building)
                .Where(p => p.IsBoosted && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(20) // Ana sayfa vitrini
                .ToListAsync();
        }

        // ✅ Aktif ilanları tüm detaylarıyla getir (Copilot N+1 Fix)
        public async Task<IEnumerable<Property>> GetActivePropertiesWithDetailsAsync(int page = 1, int pageSize = 20)
        {
            return await _context.Properties
                .Include(p => p.Building)
                    .ThenInclude(b => b.City)
                .Include(p => p.Building)
                    .ThenInclude(b => b.District)
                .Include(p => p.Building)
                    .ThenInclude(b => b.Neighborhood)
                .Include(p => p.Images.OrderBy(i => i.SortOrder).Take(5))
                .Include(p => p.User)
                .Where(p => p.State == GMK360.Core.Entities.ListingState.Active && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking() // ✅ Read-only = daha hızlı
                .ToListAsync();
        }

        // ✅ Tek bir ilanı tüm detaylarıyla getir (Copilot N+1 Fix)
        public async Task<Property?> GetPropertyByIdWithDetailsAsync(int id)
        {
            return await _context.Properties
                .Include(p => p.Building)
                    .ThenInclude(b => b.City)
                .Include(p => p.Building)
                    .ThenInclude(b => b.District)
                .Include(p => p.Building)
                    .ThenInclude(b => b.Neighborhood)
                .Include(p => p.Building)
                    .ThenInclude(b => b.Street)
                .Include(p => p.Images.OrderBy(i => i.SortOrder))
                .Include(p => p.Features)
                .Include(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }
    }
}
