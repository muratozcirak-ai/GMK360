using System.Collections.Generic;
using System.Threading.Tasks;
using GMK360.Core.DTOs;
using GMK360.Core.Entities;

namespace GMK360.Core.Services
{
    public interface IPropertySearchService
    {
        Task<List<Property>> SearchPropertiesAsync(PropertySearchFilterDto filter);
    }
}
