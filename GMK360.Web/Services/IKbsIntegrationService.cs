using System.Threading.Tasks;

namespace GMK360.Web.Services
{
    public interface IKbsIntegrationService
    {
        Task<(bool IsSuccess, string ErrorMessage)> TransmitToEgmAsync(int reservationId);
    }
}
