using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Services
{
    public class WalletService
    {
        private readonly ApplicationDbContext _context;

        public WalletService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Kullanıcının bakiyesinden belirtilen tutarı düşer. 
        /// Her zaman ÖNCE Hediye Bakiyeyi kullanır, yetmezse Gerçek Paradan çeker.
        /// </summary>
        /// <param name="userId">Kullanıcı ID</param>
        /// <param name="amountToSpend">Harcancak Toplam Tutar</param>
        /// <returns>İşlem başarılıysa true, bakiye yetersizse false</returns>
        public async Task<bool> SpendCreditAsync(string userId, decimal amountToSpend)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            // 1. Toplam bakiye kontrolü
            decimal totalAvailableBalance = user.GiftBalance + user.RealMoneyBalance;
            if (totalAvailableBalance < amountToSpend)
            {
                return false; // Bakiye yetersiz
            }

            // 2. Önce Hediye Kredileri harca (Çünkü iade edilemez ve sürelidir)
            decimal remainingToSpend = amountToSpend;

            if (user.GiftBalance > 0)
            {
                if (user.GiftBalance >= remainingToSpend)
                {
                    // Hediye bakiye her şeyi karşılıyor
                    user.GiftBalance -= remainingToSpend;
                    remainingToSpend = 0;
                }
                else
                {
                    // Hediye bakiye tamamen sıfırlanıyor, kalanı gerçek paradan düşecek
                    remainingToSpend -= user.GiftBalance;
                    user.GiftBalance = 0;
                }
            }

            // 3. Eğer hala ödenecek tutar kaldıysa, Gerçek Paradan düş
            if (remainingToSpend > 0)
            {
                user.RealMoneyBalance -= remainingToSpend;
            }

            // 4. İlgili cüzdan hareketini (Log) WalletCredit tablosuna yazabiliriz
            var historyLog = new WalletCredit
            {
                UserId = userId,
                Amount = -amountToSpend,
                Source = "Purchase (Vitrin/Doping)",
                ExpiryDate = DateTime.UtcNow,
                IsUsed = true
            };
            
            _context.WalletCredits.Add(historyLog);

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Kullanıcıya Hediye (Bonus) Kredi tanımlar
        /// </summary>
        public async Task AddGiftCreditAsync(string userId, decimal amount, int validityDays, string source)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return;

            user.GiftBalance += amount;

            var credit = new WalletCredit
            {
                UserId = userId,
                Amount = amount,
                Source = source,
                ExpiryDate = DateTime.UtcNow.AddDays(validityDays),
                IsUsed = false
            };

            _context.WalletCredits.Add(credit);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Kullanıcı hesabına gerçek para (kredi kartı ile yüklenmiş) ekler
        /// </summary>
        public async Task AddRealMoneyAsync(string userId, decimal amount)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return;

            user.RealMoneyBalance += amount;
            
            var credit = new WalletCredit
            {
                UserId = userId,
                Amount = amount,
                Source = "Credit Card Deposit",
                ExpiryDate = DateTime.UtcNow.AddYears(10), // Gerçek paranın süresi olmaz (veya çok uzun tutulur)
                IsUsed = false
            };

            _context.WalletCredits.Add(credit);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Davet Kodu (Referans) ile kayıt olan Usta/Esnaf sisteme ödeme yaptığında, davet edene prim yükler (60 Gün Provizyon).
        /// </summary>
        public async Task ProcessReferralCommissionAsync(string newUserId, decimal grossCommissionAmount)
        {
            var newUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == newUserId);
            if (newUser == null || string.IsNullOrEmpty(newUser.ReferredByUserId))
                return;

            var referrer = await _context.Users.FirstOrDefaultAsync(u => u.Id == newUser.ReferredByUserId);
            if (referrer == null)
                return;

            decimal taxRate = 0.20m; // %20 Stopaj
            decimal taxAmount = grossCommissionAmount * taxRate;
            decimal netAmount = grossCommissionAmount - taxAmount;

            var fiatTx = new FiatTransaction
            {
                UserId = referrer.Id,
                GrossAmount = grossCommissionAmount,
                TaxAmount = taxAmount,
                NetAmount = netAmount,
                Status = "Pending", // 60 gün boyunca provizyonda bekleyecek
                ClearsAt = DateTime.UtcNow.AddDays(60),
                Description = "Referral Commission from user " + newUser.Id
            };

            _context.FiatTransactions.Add(fiatTx);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 60 günlük provizyon süresi dolan komisyonları kesinleştirir ve kullanıcının net bakiyesine ekler.
        /// </summary>
        public async Task ClearPendingCommissionsAsync()
        {
            var pendingTxs = await _context.FiatTransactions
                .Where(t => t.Status == "Pending" && t.ClearsAt <= DateTime.UtcNow)
                .Include(t => t.User)
                .ToListAsync();

            foreach (var tx in pendingTxs)
            {
                tx.Status = "Cleared";
                tx.User.RealMoneyBalance += tx.NetAmount; // Sadece Net Bakiye yansıtılır (Stopaj kesilmiş hali)
                
                // İsteğe bağlı olarak WalletCredit tablosuna da log atılabilir
                var creditLog = new WalletCredit
                {
                    UserId = tx.UserId,
                    Amount = tx.NetAmount,
                    Source = "Cleared Referral Commission",
                    ExpiryDate = DateTime.UtcNow.AddYears(10),
                    IsUsed = false
                };
                _context.WalletCredits.Add(creditLog);
            }

            if (pendingTxs.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Kullanıcının nakit çekim talebini oluşturur (IBAN vb.). Limit 2000 TL'dir.
        /// </summary>
        public async Task<(bool Success, string Message)> RequestWithdrawalAsync(string userId, decimal amount)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return (false, "Kullanıcı bulunamadı.");

            if (user.RealMoneyBalance < 2000)
                return (false, "Nakit çekim talebi için bakiyeniz en az 2.000 TL olmalıdır.");

            if (user.RealMoneyBalance < amount)
                return (false, "Çekim talebi bakiyenizden büyük olamaz.");

            // Talebi veritabanına ekle
            var request = new WalletWithdrawalRequest
            {
                UserId = userId,
                Amount = amount,
                RequestDate = DateTime.UtcNow,
                IsProcessed = false
            };

            _context.WalletWithdrawalRequests.Add(request);
            
            // Kullanıcının bakiyesinden bu tutarı blokaj (düşme) olarak yapıyoruz.
            user.RealMoneyBalance -= amount;

            await _context.SaveChangesAsync();

            return (true, "Nakit çekim talebiniz başarıyla alındı.");
        }

        /// <summary>
        /// İç Hizmet Satın Alımlarında (VIP SMS, Vitrin) Cüzdan Hiyerarşisi Hesaplar
        /// </summary>
        public async Task<(bool CanCoverFully, decimal WalletDeduction, decimal RemainingAmount)> CalculateHybridPaymentAsync(string userId, decimal totalAmount)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return (false, 0, totalAmount);

            decimal totalWallet = user.GiftBalance + user.RealMoneyBalance;

            if (totalWallet >= totalAmount)
            {
                return (true, totalAmount, 0); // Tamamen cüzdandan karşılanabilir
            }

            // Parçalı karşılama
            decimal remaining = totalAmount - totalWallet;

            // Iyzico gibi ödeme sistemlerinde minimum çekim tutarı genelde 1 TL (veya benzeri) olur.
            // Eğer kalan miktar 1 TL'nin altındaysa (örn 0.50 TL kalıyorsa), cüzdandan daha az düşüp, kalanı 1 TL'ye tamamlayabiliriz
            // ya da direkt cüzdanı tam sıfırlamayız. Bu örnekte Iyzico için kalan <= 1 TL ise, kalanı 1 TL'ye yuvarlıyoruz
            if (remaining > 0 && remaining < 1.00m)
            {
                decimal diff = 1.00m - remaining; // Örn 0.50 daha lazım
                if (totalWallet >= diff)
                {
                    totalWallet -= diff; // Cüzdandan 0.50 az çekelim ki kredi kartına tam 1 TL kalsın
                    remaining = 1.00m;
                }
            }

            return (false, totalWallet, remaining);
        }

        /// <summary>
        /// Sadece belirtilen tutar kadar cüzdandan düşer ve log atar. (Parçalı ödemenin başarılı olması sonrasında çağrılır)
        /// </summary>
        public async Task<bool> ProcessHybridCheckoutAsync(string userId, decimal walletDeductionAmount, string sourceDescription)
        {
            if (walletDeductionAmount <= 0) return true;

            return await SpendCreditAsync(userId, walletDeductionAmount);
        }
    }
}
