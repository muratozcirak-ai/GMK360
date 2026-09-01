using System.Threading.Tasks;

namespace GMK360.Core.Interfaces
{
    public interface IWebhookService
    {
        Task<bool> SendFaultReportWebhookAsync(object faultData);
        Task<bool> SendValuationReportWebhookAsync(object valuationData);
        Task<bool> SendProfessionalMatchWebhookAsync(object matchData);
    }
}
