using System.Collections.Generic;
using System.Threading.Tasks;
using GMK360.Core.DTOs;

namespace GMK360.Core.Interfaces
{
    public interface ITkgmService
    {
        Task<TkgmParcelResultDto> GetParcelByCoordinatesAsync(double latitude, double longitude);
        Task<TkgmParcelResultDto> GetParcelByAdaParselAsync(long mahalleId, string ada, string parsel);
        Task<List<TkgmAdministrativeUnitDto>> GetProvincesAsync();
        Task<List<TkgmAdministrativeUnitDto>> GetDistrictsAsync(int provinceId);
        Task<List<TkgmAdministrativeUnitDto>> GetNeighborhoodsAsync(int districtId);
    }
}
