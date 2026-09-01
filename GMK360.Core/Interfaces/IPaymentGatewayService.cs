using System.Threading.Tasks;
using GMK360.Core.Entities;

namespace GMK360.Core.Interfaces
{
    public interface IPaymentGatewayService
    {
        Task<bool> RegisterSubMerchantAsync(string userId, string companyName, string iban, string taxNumber, string taxOffice);
        Task<PaymentTransaction> ProcessMarketplacePaymentAsync(string senderUserId, int receiverAccountId, decimal amount, PaymentContextType contextType, System.Guid? referenceId);
        Task<PaymentTransaction> ProcessRentPaymentAsync(string tenantUserId, int landlordAccountId, decimal amount, System.Guid referenceId);
        Task<PaymentTransaction> ProcessDuesPaymentAsync(string tenantUserId, int managementAccountId, decimal amount, System.Guid referenceId);
    }
}
