using System.Collections.Generic;
using System.Threading.Tasks;
using GMK360.Core.Entities;

namespace GMK360.Core.Interfaces
{
    public interface ILocationService
    {
        Task<IEnumerable<City>> GetCitiesAsync();
        Task<IEnumerable<District>> GetDistrictsByCityIdAsync(int cityId);
        Task<IEnumerable<Neighborhood>> GetNeighborhoodsByDistrictIdAsync(int districtId);
        Task<IEnumerable<Street>> GetStreetsByNeighborhoodIdAsync(int neighborhoodId);
    }
}
