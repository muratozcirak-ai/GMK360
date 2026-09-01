using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;

namespace GMK360.Core.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly IRepository<Property> _propertyRepository;

        public PropertyService(IRepository<Property> propertyRepository)
        {
            _propertyRepository = propertyRepository;
        }

        public async Task<Property> GetPropertyByIdAsync(int id)
        {
            return await _propertyRepository.GetByIdAsync(id);
        }

        public async Task<IReadOnlyList<Property>> GetPropertiesAsync()
        {
            var props = await _propertyRepository.GetAllAsync();
            return props.OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<IReadOnlyList<Property>> GetFeaturedPropertiesAsync()
        {
            var props = await _propertyRepository.GetAsync(p => p.IsBoosted);
            return props.OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<Property> CreatePropertyAsync(Property property)
        {
            return await _propertyRepository.AddAsync(property);
        }

        public async Task UpdatePropertyAsync(Property property)
        {
            await _propertyRepository.UpdateAsync(property);
        }

        public async Task DeletePropertyAsync(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property != null)
            {
                await _propertyRepository.DeleteAsync(property);
            }
        }

        public async Task<IReadOnlyList<Property>> GetPendingPropertiesAsync()
        {
            var props = await _propertyRepository.GetAsync(p => p.State == ListingState.PendingApproval);
            return props.OrderBy(p => p.CreatedAt).ToList();
        }

        public async Task ApprovePropertyAsync(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property != null)
            {
                property.State = ListingState.Active;
                await _propertyRepository.UpdateAsync(property);
            }
        }

        public async Task RejectPropertyAsync(int id, string reason)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property != null)
            {
                property.State = ListingState.Passive;
                // We should probably save the reason somewhere, like a moderation log, but for now we'll just set it to Passive.
                await _propertyRepository.UpdateAsync(property);
            }
        }
    }
}
