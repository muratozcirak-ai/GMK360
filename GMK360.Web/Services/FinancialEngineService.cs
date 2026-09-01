using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;

namespace GMK360.Web.Services
{
    public class FinancialEngineService : IFinancialEngineService
    {
        private readonly ApplicationDbContext _context;

        public FinancialEngineService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EscrowTransaction> CreateEscrowAsync(int paymentTransactionId)
        {
            var payment = await _context.PaymentTransactions
                .Include(p => p.SenderUser)
                .FirstOrDefaultAsync(p => p.Id == paymentTransactionId);

            if (payment == null)
                throw new Exception("Ödeme işlemi bulunamadı.");

            // 1. Ayarları Getir
            var settings = await _context.GlobalFinanceSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new GlobalFinanceSettings(); // Varsayılanlar
                _context.GlobalFinanceSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            var escrow = new EscrowTransaction
            {
                PaymentTransactionId = payment.Id,
                TotalAmount = payment.Amount,
                PlatformCommissionGross = payment.PlatformCommissionAmount,
                ReleaseDate = DateTime.UtcNow.AddDays(settings.EscrowHoldDays),
                Status = EscrowStatus.Pending
            };

            // Hangi servise/ilan sahibine gidiyor? (Örnek olarak ReceiverAccount'a bağlı kullanıcıyı bulmamız lazım, 
            // şimdilik NetReceiverAmount üzerinden satıcı hak edişini hesaplıyoruz).
            var sellerProfile = await GetFinancialProfileAsync(payment.ReceiverAccount?.AppUserId); 
            escrow.SellerUserId = payment.ReceiverAccount?.AppUserId; 
            
            // Eğer Seller null ise veya ReceiverAccount yoksa, satıcı payını direkt Platforma yazabiliriz.
            if (!string.IsNullOrEmpty(escrow.SellerUserId))
            {
                escrow.SellerGrossAmount = payment.NetReceiverAmount;
                // Vergi hesapla
                escrow.SellerTaxAmount = CalculateWithholdingTax(escrow.SellerGrossAmount, sellerProfile, settings);
                escrow.SellerNetAmount = escrow.SellerGrossAmount - escrow.SellerTaxAmount;
            }

            // 2. Referans Komisyonu Dağıtımı (%25 Platform Gelirinden)
            if (!string.IsNullOrEmpty(payment.SenderUser?.ReferredByUserId))
            {
                var referrerId = payment.SenderUser.ReferredByUserId;
                var referrerProfile = await GetFinancialProfileAsync(referrerId);
                
                escrow.ReferrerUserId = referrerId;
                escrow.ReferrerGrossAmount = escrow.PlatformCommissionGross * 0.25m; // %25
                escrow.ReferrerTaxAmount = CalculateWithholdingTax(escrow.ReferrerGrossAmount, referrerProfile, settings);
                escrow.ReferrerNetAmount = escrow.ReferrerGrossAmount - escrow.ReferrerTaxAmount;
                
                // Platformun kalan brüt karını düş
                escrow.PlatformCommissionGross -= escrow.ReferrerGrossAmount;
            }

            // 3. Fonlara Dağıtım
            var activeFunds = await _context.SystemFunds.Where(f => f.IsActive).ToListAsync();
            var totalFundPercentage = activeFunds.Sum(f => f.Percentage);
            
            if (totalFundPercentage > 0 && escrow.PlatformCommissionGross > 0)
            {
                foreach (var fund in activeFunds)
                {
                    // Fonda kalan komisyonun yüzdesi kadar miktar
                    decimal amountForFund = escrow.PlatformCommissionGross * (fund.Percentage / 100m);
                    
                    var fundTx = new SystemFundTransaction
                    {
                        SystemFundId = fund.Id,
                        Amount = amountForFund
                    };
                    fund.Balance += amountForFund;
                    
                    escrow.PlatformCommissionGross -= amountForFund; // Kalan net platform karı
                    
                    _context.SystemFundTransactions.Add(fundTx);
                }
            }

            _context.EscrowTransactions.Add(escrow);
            await _context.SaveChangesAsync();

            return escrow;
        }

        public async Task ProcessPendingEscrowsAsync()
        {
            var pendingEscrows = await _context.EscrowTransactions
                .Include(e => e.SellerUser)
                .Include(e => e.ReferrerUser)
                .Where(e => e.Status == EscrowStatus.Pending && e.ReleaseDate <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var escrow in pendingEscrows)
            {
                // Satıcıya Net Tutar Aktarımı
                if (escrow.SellerUser != null && escrow.SellerNetAmount > 0)
                {
                    escrow.SellerUser.RealMoneyBalance += escrow.SellerNetAmount;
                }

                // Referans Sahibine Net Tutar Aktarımı
                if (escrow.ReferrerUser != null && escrow.ReferrerNetAmount > 0)
                {
                    escrow.ReferrerUser.RealMoneyBalance += escrow.ReferrerNetAmount;
                }

                escrow.Status = EscrowStatus.Released;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<WalletWithdrawalRequest> RequestWithdrawalAsync(string userId, decimal amount)
        {
            var settings = await _context.GlobalFinanceSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new GlobalFinanceSettings(); 
                _context.GlobalFinanceSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            if (user.RealMoneyBalance < amount)
                throw new Exception("Yetersiz bakiye.");

            if (amount < settings.MinimumWithdrawalAmount)
                throw new Exception($"Para çekme alt limiti: {settings.MinimumWithdrawalAmount:C}");

            // Bakiyeden düş
            user.RealMoneyBalance -= amount;

            var withdrawal = new WalletWithdrawalRequest
            {
                UserId = userId,
                Amount = amount,
                IsProcessed = false, 
                RequestDate = DateTime.UtcNow
            };

            _context.WalletWithdrawalRequests.Add(withdrawal);
            await _context.SaveChangesAsync();

            return withdrawal;
        }

        private async Task<UserFinancialProfile> GetFinancialProfileAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;

            var profile = await _context.UserFinancialProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                profile = new UserFinancialProfile
                {
                    UserId = userId,
                    IsCorporate = false // Varsayılan Bireysel
                };
                _context.UserFinancialProfiles.Add(profile);
            }
            return profile;
        }

        private decimal CalculateWithholdingTax(decimal grossAmount, UserFinancialProfile? profile, GlobalFinanceSettings settings)
        {
            if (profile != null && profile.IsCorporate)
            {
                // Fatura kesebilen kurumsal kullanıcı için stopaj yok (0). Faturasını kendi kesecek.
                return 0;
            }

            // Bireysel ise Stopaj uygulanır (Gider Pusulası)
            var taxRate = profile?.CustomWithholdingTaxRate ?? settings.DefaultWithholdingTaxRate;
            return grossAmount * (taxRate / 100m);
        }

        public async Task ProcessReferralBonusAsync(string sourceUserId, decimal paymentAmount, string description)
        {
            var sourceUser = await _context.Users.FindAsync(sourceUserId);
            if (sourceUser == null || string.IsNullOrEmpty(sourceUser.ReferredByUserId)) return;

            var referrer = await _context.Users.FindAsync(sourceUser.ReferredByUserId);
            if (referrer == null) return;

            var settings = await _context.GlobalFinanceSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new GlobalFinanceSettings(); 
                _context.GlobalFinanceSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            var referrerProfile = await GetFinancialProfileAsync(referrer.Id);

            // %20 Brüt Referans Geliri
            decimal grossAmount = paymentAmount * 0.20m;
            decimal taxAmount = CalculateWithholdingTax(grossAmount, referrerProfile, settings);
            decimal netAmount = grossAmount - taxAmount;

            if (netAmount > 0)
            {
                referrer.RealMoneyBalance += netAmount;
                
                var txn = new UserWalletTransaction
                {
                    ApplicationUserId = referrer.Id,
                    Amount = netAmount,
                    TransactionType = "ReferralBonus",
                    Description = description + $" (Net: {netAmount:C2} - Stopaj: {taxAmount:C2})"
                };

                _context.UserWalletTransactions.Add(txn);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AssignObligationsToPropertyAsync(int propertyId)
        {
            var property = await _context.Properties
                .Include(p => p.Type) // DefinitionValue for Property Type
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null || property.ManagementRole == null) return;

            // Determine if Commercial or Residential based on Type name
            var typeName = property.Type?.Name?.ToLower() ?? "";
            bool isCommercial = typeName.Contains("iş") || typeName.Contains("ticari") || typeName.Contains("dükkan") || typeName.Contains("ofis") || typeName.Contains("mağaza");
            bool isResidential = typeName.Contains("konut") || typeName.Contains("daire") || typeName.Contains("villa") || typeName.Contains("ev");

            // If we can't determine, assume residential
            if (!isCommercial && !isResidential) isResidential = true;

            var currentRole = property.ManagementRole == ManagementRole.Owner ? ResponsibleRole.Owner : ResponsibleRole.Tenant;

            var activeObligations = await _context.FinancialObligationTypes.Where(o => o.IsActive).ToListAsync();

            var currentYear = DateTime.UtcNow.Year;

            foreach (var obligation in activeObligations)
            {
                bool isTargetMatch = obligation.TargetPropertyType == TargetPropertyType.All || 
                                     (obligation.TargetPropertyType == TargetPropertyType.OnlyCommercial && isCommercial) ||
                                     (obligation.TargetPropertyType == TargetPropertyType.OnlyResidential && isResidential);

                bool isRoleMatch = obligation.ResponsibleRole == currentRole;

                if (isTargetMatch && isRoleMatch)
                {
                    if (obligation.PaymentFrequency == PaymentFrequency.Monthly)
                    {
                        // Stopaj gibi vergiler aylık
                        // Tüm yıl için veya kalan aylar için planla
                        for (int month = 1; month <= 12; month++)
                        {
                            await CreateScheduleIfNotExists(property.Id, property.UserId, obligation.Id, currentYear, month);
                        }
                    }
                    else if (obligation.PaymentFrequency == PaymentFrequency.Biannual)
                    {
                        if (obligation.FirstInstallmentMonth.HasValue)
                            await CreateScheduleIfNotExists(property.Id, property.UserId, obligation.Id, currentYear, obligation.FirstInstallmentMonth.Value);
                        
                        if (obligation.SecondInstallmentMonth.HasValue)
                            await CreateScheduleIfNotExists(property.Id, property.UserId, obligation.Id, currentYear, obligation.SecondInstallmentMonth.Value);
                    }
                    else if (obligation.PaymentFrequency == PaymentFrequency.Yearly)
                    {
                        int month = obligation.FirstInstallmentMonth ?? 1;
                        await CreateScheduleIfNotExists(property.Id, property.UserId, obligation.Id, currentYear, month);
                    }
                }
            }
        }

        private async Task CreateScheduleIfNotExists(int propertyId, string userId, int obligationTypeId, int year, int month)
        {
            var dueDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            var exists = await _context.PropertyFinancialSchedules
                .AnyAsync(s => s.PropertyId == propertyId && s.ObligationTypeId == obligationTypeId && s.DueDate.Year == year && s.DueDate.Month == month);

            if (!exists)
            {
                var schedule = new PropertyFinancialSchedule
                {
                    PropertyId = propertyId,
                    UserId = userId,
                    ObligationTypeId = obligationTypeId,
                    DueDate = dueDate,
                    Status = ScheduleStatus.Pending,
                    Amount = 0 // Kesin miktar belirsiz, kullanıcı öderken girecek
                };
                _context.PropertyFinancialSchedules.Add(schedule);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IncomeTaxDeclaration> SimulateMarchIncomeTaxAsync(string userId, int year)
        {
            var properties = await _context.Properties
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .ToListAsync();

            decimal totalRentalIncome = 0;

            foreach (var property in properties)
            {
                if (property.OccupantType == GMK360.Core.Entities.Enums.OccupantType.PayingTenant)
                {
                    // Normal kiracı ise yıllık kiranın tamamı
                    totalRentalIncome += (property.RentAmount ?? 0) * 12;
                }
                else if (property.OccupantType == GMK360.Core.Entities.Enums.OccupantType.NonExemptResident)
                {
                    // Diğer akraba/arkadaş ise emsal kira bedeli (Rayiç bedelin %5'i)
                    totalRentalIncome += (property.PropertyTaxBaseValue ?? 0) * 0.05m;
                }
                else
                {
                    // OwnerOccupied veya ExemptRelative ise gelir = 0
                    totalRentalIncome += 0;
                }
            }

            // Götürü gider (%15) düşümü ve muafiyet vs. için basitleştirilmiş örnek:
            decimal legalExemption = 33000m; // 2024 yılı istisna tutarı (Örnek)
            decimal taxableIncome = totalRentalIncome > legalExemption ? (totalRentalIncome - legalExemption) : 0;
            decimal taxBase = taxableIncome * 0.85m; // %15 götürü gider

            var declaration = new IncomeTaxDeclaration
            {
                OwnerUserId = userId,
                TaxYear = year,
                TotalRentalIncome = totalRentalIncome,
                LegalExemptionAmount = legalExemption,
                CalculatedTaxBase = taxBase,
                EstimatedTaxAmount = taxBase * 0.15m, // %15 birinci dilim tahmini
                DeductionMethod = DeductionMethod.LumpSum,
                DeclarationStatus = DeclarationStatus.Draft
            };

            return declaration;
        }
    }
}
