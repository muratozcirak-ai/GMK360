using System.Collections.Generic;
using System.Threading.Tasks;
using GMK360.Core.Entities;

namespace GMK360.Core.Interfaces
{
    public interface IPropertyRepository
    {
        Task<List<Property>> GetPropertiesByCityAsync(string city);
        Task<List<Property>> GetFeaturedPropertiesAsync();
        
        Task<IEnumerable<Property>> GetActivePropertiesWithDetailsAsync(int page = 1, int pageSize = 20);
        Task<Property?> GetPropertyByIdWithDetailsAsync(int id);
    }
}
