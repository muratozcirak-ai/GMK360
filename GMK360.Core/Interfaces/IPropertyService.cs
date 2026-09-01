using System.Collections.Generic;
using System.Threading.Tasks;
using GMK360.Core.Entities;

namespace GMK360.Core.Interfaces
{
    public interface IPropertyService
    {
        Task<Property> GetPropertyByIdAsync(int id);
        Task<IReadOnlyList<Property>> GetPropertiesAsync();
        Task<IReadOnlyList<Property>> GetFeaturedPropertiesAsync();
        Task<Property> CreatePropertyAsync(Property property);
        Task UpdatePropertyAsync(Property property);
        Task DeletePropertyAsync(int id);
        
        // Moderation
        Task<IReadOnlyList<Property>> GetPendingPropertiesAsync();
        Task ApprovePropertyAsync(int id);
        Task RejectPropertyAsync(int id, string reason);
    }
}
