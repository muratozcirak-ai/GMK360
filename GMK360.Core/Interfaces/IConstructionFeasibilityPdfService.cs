using System.Threading.Tasks;
using GMK360.Core.DTOs;

namespace GMK360.Core.Interfaces
{
    public interface IConstructionFeasibilityPdfService
    {
        byte[] GenerateFeasibilityReport(FeasibilityPdfRequestDto dto);
    }
}
