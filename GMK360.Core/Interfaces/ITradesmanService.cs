using System.Threading.Tasks;
using GMK360.Core.Entities;

namespace GMK360.Core.Interfaces
{
    public interface ITradesmanService
    {
        Task<bool> AssignJobToTradesmanAsync(int renovationRequestId, string tradesmanUserId);
        Task<bool> AssignJobToSupplierAsync(int renovationRequestId, string supplierUserId);
        Task<bool> CompleteJobAndTriggerSurveyAsync(int renovationRequestId, int rating, string comments);
    }
}
