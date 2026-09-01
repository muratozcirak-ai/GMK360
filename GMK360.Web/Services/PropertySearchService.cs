using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.DTOs;
using GMK360.Core.Entities;
using GMK360.Core.Services;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Services
{
    public class PropertySearchService : IPropertySearchService
    {
        private readonly ApplicationDbContext _context;

        public PropertySearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Property>> SearchPropertiesAsync(PropertySearchFilterDto filter)
        {
            // 1. Temel Sorgu (Eager Loading ile bağımlılıkları çek)
            IQueryable<Property> query = _context.Properties
                .Include(p => p.Complex)
                    .ThenInclude(c => c.City)
                .Include(p => p.Complex)
                    .ThenInclude(c => c.District)
                .Include(p => p.Images)
                .Include(p => p.Features)
                    .ThenInclude(f => f.DefinitionValue)
                .Where(p => !p.IsDeleted && p.State != ListingState.PrivateTracking);

            // 2. Temel Filtreler
            if (filter.CityId.HasValue)
                query = query.Where(p => p.Complex.CityId == filter.CityId.Value);

            if (filter.DistrictId.HasValue)
                query = query.Where(p => p.Complex.DistrictId == filter.DistrictId.Value);

            if (filter.PropertyTypeId.HasValue)
                query = query.Where(p => p.TypeId == filter.PropertyTypeId.Value);

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            // 3. EAV Mimarisi Dinamik Özellik Filtrelemesi (Havuzlu, Eşyalı vb.)
            if (filter.FeatureIds != null && filter.FeatureIds.Any())
            {
                // Seçilen HER BİR özelliğe sahip olan ilanları getir (AND Mantığı)
                foreach (var featureId in filter.FeatureIds)
                {
                    query = query.Where(p => p.Features.Any(f => f.DefinitionValueId == featureId));
                }
            }

            // 4. Kısa Dönem Kiralık (Turistik) için Anti-Overlap (Çakışma Önleyici) Algoritma
            if (filter.CheckInDate.HasValue && filter.CheckOutDate.HasValue)
            {
                var checkIn = filter.CheckInDate.Value;
                var checkOut = filter.CheckOutDate.Value;

                // Seçilen tarihlerle "çakışan" herhangi bir rezervasyonu olan ilanları ELİYORUZ.
                // Çakışma Formülü: (Giriş < MevcutÇıkış) VE (Çıkış > MevcutGiriş)
                query = query.Where(p => !p.Reservations.Any(r => 
                    r.Status != "Cancelled" && // İptal edilenler hariç
                    r.CheckInDate < checkOut && 
                    r.CheckOutDate > checkIn));
            }

            // 5. Dopingli (IsBoosted) ilanları üste, ardından en yenileri sırala
            query = query
                .OrderByDescending(p => p.IsBoosted)
                .ThenByDescending(p => p.CreatedAt);

            return await query.ToListAsync();
        }
    }
}
