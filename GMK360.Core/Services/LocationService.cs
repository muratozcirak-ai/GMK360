using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;

namespace GMK360.Core.Services
{
    public class LocationService : ILocationService
    {
        private readonly IRepository<City> _cityRepository;
        private readonly IRepository<District> _districtRepository;
        private readonly IRepository<Neighborhood> _neighborhoodRepository;
        private readonly IRepository<Street> _streetRepository;

        public LocationService(
            IRepository<City> cityRepository,
            IRepository<District> districtRepository,
            IRepository<Neighborhood> neighborhoodRepository,
            IRepository<Street> streetRepository)
        {
            _cityRepository = cityRepository;
            _districtRepository = districtRepository;
            _neighborhoodRepository = neighborhoodRepository;
            _streetRepository = streetRepository;
        }

        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
            return await _cityRepository.GetAllAsync();
        }

        public async Task<IEnumerable<District>> GetDistrictsByCityIdAsync(int cityId)
        {
            var all = await _districtRepository.GetAllAsync();
            return all.Where(d => d.CityId == cityId).ToList();
        }

        public async Task<IEnumerable<Neighborhood>> GetNeighborhoodsByDistrictIdAsync(int districtId)
        {
            var all = await _neighborhoodRepository.GetAllAsync();
            return all.Where(n => n.DistrictId == districtId).ToList();
        }

        public async Task<IEnumerable<Street>> GetStreetsByNeighborhoodIdAsync(int neighborhoodId)
        {
            var all = await _streetRepository.GetAllAsync();
            return all.Where(s => s.NeighborhoodId == neighborhoodId).ToList();
        }
    }
}
