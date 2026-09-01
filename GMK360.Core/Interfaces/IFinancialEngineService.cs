using System.Threading.Tasks;
using GMK360.Core.Entities;

namespace GMK360.Core.Interfaces
{
    public interface IFinancialEngineService
    {
        /// <summary>
        /// Bir ödeme geldiğinde (PaymentTransaction), parayı havuza alır, stopaj ve referans komisyonlarını hesaplar, fonlara dağıtır.
        /// </summary>
        Task<EscrowTransaction> CreateEscrowAsync(int paymentTransactionId);

        /// <summary>
        /// Yasal bekleme süresi dolmuş EscrowTransaction'ları bularak hak sahiplerinin cüzdanlarına (RealMoneyBalance) aktarır.
        /// </summary>
        Task ProcessPendingEscrowsAsync();

        /// <summary>
        /// Kullanıcının nakit çekim talebi oluşturmasını sağlar. Limit ve Stopaj/Fatura mantığı burada işler.
        /// </summary>
        Task<WalletWithdrawalRequest> RequestWithdrawalAsync(string userId, decimal amount);

        /// <summary>
        /// Bir kullanıcı abonelik satın aldığında, onu davet eden kişiye %20 referans geliri atar (stopaj düşülerek).
        /// </summary>
        Task ProcessReferralBonusAsync(string sourceUserId, decimal paymentAmount, string description);

        /// <summary>
        /// Mülk sisteme eklendiğinde veya ayarları değiştiğinde, kural motoruna (FinancialObligationTypes) göre vergileri otomatik takvime ekler.
        /// </summary>
        Task AssignObligationsToPropertyAsync(int propertyId);

        /// <summary>
        /// Mart ayında çalıştırılacak Vergi Simülasyonu.
        /// Oturan tipine göre Emsal Kira veya Akraba Muafiyetini hesaplayarak matrah çıkarır.
        /// </summary>
        Task<IncomeTaxDeclaration> SimulateMarchIncomeTaxAsync(string userId, int year);
    }
}
