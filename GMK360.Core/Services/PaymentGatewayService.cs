using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;

namespace GMK360.Core.Services
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly IRepository<FinancialAccount> _accountRepository;
        private readonly IRepository<PaymentTransaction> _transactionRepository;
        private readonly IRepository<PlatformCommissionRate> _commissionRepository;

        public PaymentGatewayService(
            IRepository<FinancialAccount> accountRepository,
            IRepository<PaymentTransaction> transactionRepository,
            IRepository<PlatformCommissionRate> commissionRepository)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _commissionRepository = commissionRepository;
        }

        public async Task<bool> RegisterSubMerchantAsync(string userId, string companyName, string iban, string taxNumber, string taxOffice)
        {
            // Gerçekte Iyzico SubMerchant kayıt API'sine gidilir, dönen SubMerchantKey kaydedilir.
            var account = new FinancialAccount
            {
                AppUserId = userId,
                AccountType = AccountType.Corporate,
                CompanyName = companyName,
                IBAN = iban,
                TaxNumber = taxNumber,
                TaxOffice = taxOffice,
                SubMerchantKey = Guid.NewGuid().ToString("N"), // Iyzico'dan geldiğini varsayıyoruz
                IsActive = true
            };

            await _accountRepository.AddAsync(account);
            return true;
        }

        public async Task<PaymentTransaction> ProcessMarketplacePaymentAsync(string senderUserId, int receiverAccountId, decimal amount, PaymentContextType contextType, Guid? referenceId)
        {
            var receiverAccount = await _accountRepository.GetByIdAsync(receiverAccountId);
            if (receiverAccount == null || !receiverAccount.IsActive)
                throw new Exception("Receiver account is not active or not found.");

            // Komisyon oranını bul
            var commissions = await _commissionRepository.GetAsync(c => c.ContextType == contextType);
            var commission = commissions.FirstOrDefault();
            
            decimal platformCut = 0;
            if (commission != null)
            {
                platformCut = (amount * commission.Percentage / 100m) + commission.FixedFee;
            }

            var transaction = new PaymentTransaction
            {
                SenderUserId = senderUserId,
                ReceiverAccountId = receiverAccountId,
                Amount = amount,
                PlatformCommissionAmount = platformCut,
                NetReceiverAmount = amount - platformCut,
                ContextType = contextType,
                ReferenceId = referenceId,
                TransactionId = Guid.NewGuid().ToString("N"), // Iyzico işlem ID'si
                Status = PaymentTransactionStatus.Completed // Simülasyon gereği direkt başarılı
            };

            await _transactionRepository.AddAsync(transaction);
            return transaction;
        }

        public async Task<PaymentTransaction> ProcessRentPaymentAsync(string tenantUserId, int landlordAccountId, decimal amount, Guid referenceId)
        {
            return await ProcessMarketplacePaymentAsync(tenantUserId, landlordAccountId, amount, PaymentContextType.Rent, referenceId);
        }

        public async Task<PaymentTransaction> ProcessDuesPaymentAsync(string tenantUserId, int managementAccountId, decimal amount, Guid referenceId)
        {
            return await ProcessMarketplacePaymentAsync(tenantUserId, managementAccountId, amount, PaymentContextType.Dues, referenceId);
        }
    }
}
