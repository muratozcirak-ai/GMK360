using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Auditing;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Services
{
    public sealed class GovernanceActionResult
    {
        public bool Success { get; set; }
        public bool PendingApproval { get; set; }
        public int? CreatedExpenseId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public sealed class ApprovalSignatureItem
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime ApprovedAtUtc { get; set; }
    }

    /// <summary>
    /// Dönemsel cari izolasyonu (ResidencyPeriods), kıstelyevm, ikili cari gider dağıtımı,
    /// fiziksel silmesiz Ters Kayıt (Reversal) ve 2 Aza (Multi-Sig) onay akışlarını yönetir.
    /// </summary>
    public class FacilityGovernanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public FacilityGovernanceService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        public static decimal CalculateProratedAmount(decimal monthlyAmount, DateTime moveDate, bool isLeaving)
        {
            if (monthlyAmount <= 0) return 0m;
            int daysInMonth = DateTime.DaysInMonth(moveDate.Year, moveDate.Month);
            int billableDays = isLeaving ? moveDate.Day : (daysInMonth - moveDate.Day + 1);
            billableDays = Math.Clamp(billableDays, 1, daysInMonth);
            return Math.Round((monthlyAmount / daysInMonth) * billableDays, 2);
        }

        public async Task<int> GetActiveBoardMemberCountAsync(int rootBuildingId)
        {
            return await _context.BuildingManagers.CountAsync(m =>
                m.BuildingId == rootBuildingId
                && m.IsActive
                && m.Role == BuildingManagerRole.BoardMember);
        }

        public async Task<ResidencyPeriod?> EnsureActivePeriodFromUnitAsync(BuildingUnit unit, string roleType)
        {
            var existing = await _context.ResidencyPeriods
                .FirstOrDefaultAsync(rp => rp.BuildingUnitId == unit.Id && rp.RoleType == roleType && rp.Status == "ACTIVE");
            if (existing != null) return existing;

            bool isOwner = string.Equals(roleType, "Owner", StringComparison.OrdinalIgnoreCase);
            string? fullName = isOwner ? unit.OwnerName : unit.TenantName;
            string? phone = isOwner ? unit.OwnerPhone : unit.TenantPhone;
            string? email = isOwner ? unit.OwnerEmail : unit.TenantEmail;
            string? userId = isOwner ? unit.OwnerUserId : unit.TenantUserId;

            if (string.IsNullOrWhiteSpace(fullName) && string.IsNullOrWhiteSpace(userId))
                return null;

            var period = new ResidencyPeriod
            {
                BuildingUnitId = unit.Id,
                ResidentUserId = userId,
                RoleType = isOwner ? "Owner" : "Tenant",
                FullName = string.IsNullOrWhiteSpace(fullName) ? (isOwner ? "Mal Sahibi" : "Kiracı") : fullName.Trim(),
                Phone = phone?.Trim(),
                Email = email?.Trim(),
                Status = "ACTIVE",
                StartDate = DateTime.UtcNow
            };
            _context.ResidencyPeriods.Add(period);
            await _context.SaveChangesAsync();
            return period;
        }

        public async Task<GovernanceActionResult> DistributeExpenseAsync(
            int buildingId,
            string description,
            decimal totalAmount,
            BuildingExpenseType expenseType,
            IFormFile? invoiceFile,
            string actorUserId,
            string distributionRule = "Esit")
        {
            if (totalAmount <= 0)
                return new GovernanceActionResult { Success = false, Message = "Gider tutarı sıfırdan büyük olmalıdır." };

            string? invoiceUrl = null;
            if (invoiceFile != null && invoiceFile.Length > 0)
            {
                var ext = Path.GetExtension(invoiceFile.FileName)?.ToLowerInvariant() ?? "";
                var allowed = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(ext))
                    return new GovernanceActionResult { Success = false, Message = "Fatura belgesi yalnızca PDF veya görsel olabilir." };

                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var folder = Path.Combine(webRoot, "uploads", "buildings", buildingId.ToString(), "invoices");
                Directory.CreateDirectory(folder);
                var fileName = $"fatura_{buildingId}_{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(folder, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await invoiceFile.CopyToAsync(stream);
                }
                invoiceUrl = $"/uploads/buildings/{buildingId}/invoices/{fileName}";
            }

            bool isCapital = expenseType == BuildingExpenseType.CapitalExpenditure;
            var expense = new BuildingExpense
            {
                BuildingId = buildingId,
                ScopeType = ExpenseScopeType.Building,
                ExpenseType = expenseType,
                PayerType = isCapital ? "Owner" : "Tenant",
                Description = description.Trim(),
                TotalAmount = totalAmount,
                InvoiceDocumentUrl = invoiceUrl,
                Date = DateTime.Now
            };
            _context.BuildingExpenses.Add(expense);
            await _context.SaveChangesAsync();

            var units = await _context.BuildingUnits
                .Where(u => u.BuildingId == buildingId && u.Category != "OrtakAlan_Blok" && (u.Category == null || !u.Category.StartsWith("OrtakAlan")))
                .ToListAsync();

            if (units.Any())
            {
                bool useLandShare = string.Equals(distributionRule, "ArsaPayi", StringComparison.OrdinalIgnoreCase)
                    && units.All(u => u.LandShare > 0);
                bool useAreaWeight = !useLandShare
                    && (string.Equals(distributionRule, "Metrekare", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(distributionRule, "ArsaPayi", StringComparison.OrdinalIgnoreCase))
                    && units.All(u => (u.GrossSquareMeters ?? 0) > 0);

                decimal totalLandShare = useLandShare ? units.Sum(u => u.LandShare) : 0m;
                double totalArea = useAreaWeight ? units.Sum(u => u.GrossSquareMeters ?? 0) : 0;

                foreach (var unit in units)
                {
                    decimal shareAmount = useLandShare && totalLandShare > 0
                        ? Math.Round(totalAmount * (unit.LandShare / totalLandShare), 2)
                        : (useAreaWeight && totalArea > 0
                            ? Math.Round(totalAmount * (decimal)((unit.GrossSquareMeters ?? 0) / totalArea), 2)
                            : Math.Round(totalAmount / units.Count, 2));

                    var ownerPeriod = await EnsureActivePeriodFromUnitAsync(unit, "Owner");
                    var tenantPeriod = isCapital ? null : await EnsureActivePeriodFromUnitAsync(unit, "Tenant");

                    var targetPeriod = isCapital ? ownerPeriod : (tenantPeriod ?? ownerPeriod);
                    string payerRole = isCapital ? "Owner" : (tenantPeriod != null ? "Tenant" : "Owner");

                    _context.UnitDebts.Add(new UnitDebt
                    {
                        BuildingUnitId = unit.Id,
                        BuildingExpenseId = expense.Id,
                        ResidencyPeriodId = targetPeriod?.Id,
                        PayerRole = payerRole,
                        Amount = shareAmount,
                        IsPaid = false
                    });
                }
                await _context.SaveChangesAsync();
            }

            WriteAuditLog(actorUserId, "EXPENSE_CREATED", "BuildingExpense", expense.Id.ToString(), new
            {
                buildingId,
                expense.TotalAmount,
                ExpenseType = expense.ExpenseType.ToString(),
                DistributionRule = distributionRule,
                expense.InvoiceDocumentUrl
            });
            await _context.SaveChangesAsync();

            return new GovernanceActionResult
            {
                Success = true,
                CreatedExpenseId = expense.Id,
                Message = isCapital
                    ? "Demirbaş gideri işlendi ve yalnız Mal Sahiplerinin carisine paylaştırıldı."
                    : "İşletme aidat gideri işlendi (kiracısı olan dairede kiracıya, boş/malik oturan dairede malike yansıtıldı)."
            };
        }

        public async Task<GovernanceActionResult> TransferResidencyAsync(
            int unitId,
            string roleType,
            string? newFullName,
            string? newPhone,
            string? newEmail,
            DateTime transferDate,
            decimal? monthlyDuesForProration,
            string actorUserId)
        {
            var unit = await _context.BuildingUnits.Include(u => u.Building).FirstOrDefaultAsync(u => u.Id == unitId);
            if (unit == null)
                return new GovernanceActionResult { Success = false, Message = "Bağımsız bölüm bulunamadı." };

            bool isOwner = string.Equals(roleType, "Owner", StringComparison.OrdinalIgnoreCase);
            string normalizedRole = isOwner ? "Owner" : "Tenant";
            int rootBuildingId = unit.Building?.ParentBuildingId ?? unit.BuildingId;

            await using var tx = await _context.Database.BeginTransactionAsync();

            var activePeriod = await EnsureActivePeriodFromUnitAsync(unit, normalizedRole);
            decimal unpaidTotal = 0m;
            bool openedDebtApproval = false;

            if (activePeriod != null)
            {
                var periodUnpaidDebts = await _context.UnitDebts
                    .Where(d => d.BuildingUnitId == unitId && !d.IsPaid && !d.IsArchived && d.Amount > 0
                        && (d.ResidencyPeriodId == activePeriod.Id || (d.ResidencyPeriodId == null && d.PayerRole == normalizedRole)))
                    .ToListAsync();

                foreach (var d in periodUnpaidDebts)
                {
                    if (!d.ResidencyPeriodId.HasValue) d.ResidencyPeriodId = activePeriod.Id;
                }
                unpaidTotal = periodUnpaidDebts.Sum(d => d.Amount);

                // Kıstelyevm hesabı (yalnız aylık işletme aidatı verildiyse bilgilendirme/ek borç)
                decimal proratedAmount = 0m;
                if (!isOwner && monthlyDuesForProration.HasValue && monthlyDuesForProration.Value > 0)
                {
                    proratedAmount = CalculateProratedAmount(monthlyDuesForProration.Value, transferDate, isLeaving: true);
                }

                activePeriod.Status = "ARCHIVED";
                activePeriod.EndDate = transferDate;
                activePeriod.ArchivedAt = DateTime.UtcNow;
                activePeriod.ArchivedByUserId = actorUserId;
                activePeriod.ArchiveNote = unpaidTotal > 0
                    ? $"Devir sırasında {unpaidTotal:N2} TL ödenmemiş borç eski dönemde donduruldu." + (proratedAmount > 0 ? $" Kıstelyevm hesabı: {proratedAmount:N2} TL." : "")
                    : (proratedAmount > 0 ? $"Temiz devir. Kıstelyevm son ay payı: {proratedAmount:N2} TL." : "Borçsuz temiz devir.");

                if (!string.IsNullOrEmpty(activePeriod.ResidentUserId))
                {
                    bool stillActiveElsewhere = await _context.ResidencyPeriods
                        .AnyAsync(rp => rp.ResidentUserId == activePeriod.ResidentUserId && rp.Status == "ACTIVE" && rp.Id != activePeriod.Id);
                    if (!stillActiveElsewhere)
                    {
                        var oldUser = await _userManager.FindByIdAsync(activePeriod.ResidentUserId);
                        if (oldUser != null) await _userManager.UpdateSecurityStampAsync(oldUser);
                    }
                }

                if (unpaidTotal > 0 && await GetActiveBoardMemberCountAsync(rootBuildingId) >= 2)
                {
                    openedDebtApproval = true;
                    _context.ApprovalRequests.Add(new ApprovalRequest
                    {
                        BuildingId = rootBuildingId,
                        RequestedByUserId = actorUserId,
                        RequestType = "DEBT_TRANSFER",
                        TargetEntityName = "ResidencyPeriod",
                        TargetEntityId = activePeriod.Id,
                        PayloadJson = JsonSerializer.Serialize(new { unitId, unit.DoorNumber, RoleType = normalizedRole, UnpaidFrozenAmount = unpaidTotal }),
                        Reason = $"Daire {unit.DoorNumber} ({normalizedRole}) devrinde {unpaidTotal:N2} TL ödenmemiş borç donduruldu.",
                        Status = "PENDING",
                        RequiredApprovals = 2
                    });
                }
            }

            ApplicationUser? linkedNewUser = null;
            if (!string.IsNullOrWhiteSpace(newEmail))
            {
                linkedNewUser = await _userManager.FindByEmailAsync(newEmail.Trim());
            }

            ResidencyPeriod? newPeriod = null;
            if (!string.IsNullOrWhiteSpace(newFullName))
            {
                newPeriod = new ResidencyPeriod
                {
                    BuildingUnitId = unitId,
                    ResidentUserId = linkedNewUser?.Id,
                    RoleType = normalizedRole,
                    FullName = newFullName.Trim(),
                    Phone = newPhone?.Trim(),
                    Email = newEmail?.Trim(),
                    Status = "ACTIVE",
                    StartDate = transferDate
                };
                _context.ResidencyPeriods.Add(newPeriod);
            }

            // BuildingUnit aktif görünümünü güncelle (Yeni sakin 0,00 TL bakiye ile başlar)
            if (isOwner)
            {
                unit.OwnerName = newFullName?.Trim();
                unit.OwnerPhone = newPhone?.Trim();
                unit.OwnerEmail = newEmail?.Trim();
                unit.OwnerUserId = linkedNewUser?.Id;
            }
            else
            {
                unit.TenantName = newFullName?.Trim();
                unit.TenantPhone = newPhone?.Trim();
                unit.TenantEmail = newEmail?.Trim();
                unit.TenantUserId = linkedNewUser?.Id;
                unit.IsEmpty = string.IsNullOrWhiteSpace(newFullName);
            }

            WriteAuditLog(actorUserId, "RESIDENCY_TRANSFER", "BuildingUnit", unitId.ToString(), new
            {
                Role = normalizedRole,
                ArchivedPeriodId = activePeriod?.Id,
                UnpaidFrozenAmount = unpaidTotal,
                NewResidentName = newFullName,
                TransferDate = transferDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            var msg = newPeriod != null
                ? $"Daire {unit.DoorNumber} devri tamamlandı. Yeni { (isOwner ? "mal sahibi" : "kiracı") } 0,00 TL bakiye ile açıldı."
                : $"Daire {unit.DoorNumber} eski kaydı arşivlendi.";
            if (unpaidTotal > 0)
            {
                msg += openedDebtApproval
                    ? $" Önceki döneme ait {unpaidTotal:N2} TL borç eski caride dondurulup 2 Aza onayına sunuldu."
                    : $" Önceki döneme ait {unpaidTotal:N2} TL borç eski dönemde donduruldu.";
            }

            return new GovernanceActionResult { Success = true, PendingApproval = openedDebtApproval, Message = msg };
        }

        public async Task<GovernanceActionResult> RequestOrExecuteExpenseReversalAsync(int expenseId, string reason, string actorUserId)
        {
            var expense = await _context.BuildingExpenses.FirstOrDefaultAsync(e => e.Id == expenseId);
            if (expense == null)
                return new GovernanceActionResult { Success = false, Message = "Gider kaydı bulunamadı." };
            if (expense.IsArchived || expense.TotalAmount < 0 || expense.ReversalOfExpenseId.HasValue)
                return new GovernanceActionResult { Success = false, Message = "Bu kayıt zaten ters kayıtla iptal edilmiş." };
            if (!expense.BuildingId.HasValue)
                return new GovernanceActionResult { Success = false, Message = "Bina referansı eksik." };

            var building = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == expense.BuildingId.Value);
            int rootId = building?.ParentBuildingId ?? expense.BuildingId.Value;

            bool alreadyPending = await _context.ApprovalRequests.AnyAsync(a =>
                a.BuildingId == rootId && a.RequestType == "REVERSE_EXPENSE" && a.TargetEntityId == expenseId && a.Status == "PENDING");
            if (alreadyPending)
                return new GovernanceActionResult { Success = false, Message = "Bu fatura için zaten Aza onayı bekleyen bir iptal talebi var." };

            int boardCount = await GetActiveBoardMemberCountAsync(rootId);
            if (boardCount >= 2)
            {
                _context.ApprovalRequests.Add(new ApprovalRequest
                {
                    BuildingId = rootId,
                    RequestedByUserId = actorUserId,
                    RequestType = "REVERSE_EXPENSE",
                    TargetEntityName = "BuildingExpense",
                    TargetEntityId = expenseId,
                    PayloadJson = JsonSerializer.Serialize(new { expense.Description, expense.TotalAmount }),
                    Reason = string.IsNullOrWhiteSpace(reason) ? "Hatalı gider kaydı ters kaydı" : reason.Trim(),
                    Status = "PENDING",
                    RequiredApprovals = 2
                });
                await _context.SaveChangesAsync();
                return new GovernanceActionResult
                {
                    Success = true,
                    PendingApproval = true,
                    Message = "Fiziksel silme yerine Ters Kayıt talebi açıldı. 2 Aza onayladığında otomatik işlenecektir."
                };
            }

            await ExecuteExpenseReversalInternalAsync(expense, reason, actorUserId);
            return new GovernanceActionResult
            {
                Success = true,
                Message = "Fiziksel silme yapılmadan eksi (-) tutarlı Ters Kayıt oluşturuldu ve denetim izine işlendi."
            };
        }

        public async Task<GovernanceActionResult> SubmitApprovalDecisionAsync(int requestId, string approverUserId, string approverEmail, bool approve, bool isAdmin)
        {
            var req = await _context.ApprovalRequests.FirstOrDefaultAsync(r => r.Id == requestId);
            if (req == null)
                return new GovernanceActionResult { Success = false, Message = "Onay talebi bulunamadı." };
            if (req.Status != "PENDING")
                return new GovernanceActionResult { Success = false, Message = "Bu talep zaten sonuçlanmış." };

            bool isBoardOrAdmin = isAdmin || await _context.BuildingManagers.AnyAsync(m =>
                m.BuildingId == req.BuildingId && m.UserId == approverUserId && m.IsActive && m.Role == BuildingManagerRole.BoardMember);
            if (!isBoardOrAdmin)
                return new GovernanceActionResult { Success = false, Message = "Bu işlem için aktif Aza (Yönetim Kurulu Üyesi) yetkisi gerekir." };

            if (!approve)
            {
                req.Status = "REJECTED";
                req.ExecutedAt = DateTime.UtcNow;
                WriteAuditLog(approverUserId, "APPROVAL_REJECTED", "ApprovalRequest", req.Id.ToString(), new { req.RequestType, req.TargetEntityId });
                await _context.SaveChangesAsync();
                return new GovernanceActionResult { Success = true, Message = "Talep reddedildi." };
            }

            var sigs = JsonSerializer.Deserialize<List<ApprovalSignatureItem>>(req.ApprovedByJson) ?? new List<ApprovalSignatureItem>();
            if (sigs.Any(s => s.UserId == approverUserId))
                return new GovernanceActionResult { Success = false, Message = "Bu talep için zaten onay verdiniz." };

            sigs.Add(new ApprovalSignatureItem { UserId = approverUserId, Email = approverEmail, ApprovedAtUtc = DateTime.UtcNow });
            req.ApprovedByJson = JsonSerializer.Serialize(sigs);
            req.ApprovalCount = sigs.Count;

            if (req.ApprovalCount >= req.RequiredApprovals)
            {
                if (req.RequestType == "REVERSE_EXPENSE")
                {
                    var expense = await _context.BuildingExpenses.FirstOrDefaultAsync(e => e.Id == req.TargetEntityId);
                    if (expense != null && !expense.IsArchived)
                    {
                        await ExecuteExpenseReversalInternalAsync(expense, req.Reason, approverUserId);
                    }
                }
                req.Status = "EXECUTED";
                req.ExecutedAt = DateTime.UtcNow;
            }

            WriteAuditLog(approverUserId, "APPROVAL_SIGNED", "ApprovalRequest", req.Id.ToString(), new { req.ApprovalCount, req.RequiredApprovals, req.Status });
            await _context.SaveChangesAsync();

            return new GovernanceActionResult
            {
                Success = true,
                PendingApproval = req.Status == "PENDING",
                Message = req.Status == "EXECUTED"
                    ? "Gerekli Aza onayları tamamlandı ve işlem yürütüldü."
                    : $"Onayınız kaydedildi ({req.ApprovalCount}/{req.RequiredApprovals})."
            };
        }

        private async Task ExecuteExpenseReversalInternalAsync(BuildingExpense original, string reason, string actorUserId)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            original.IsArchived = true;
            original.ArchivedAt = DateTime.UtcNow;
            original.ArchivedByUserId = actorUserId;

            var reversal = new BuildingExpense
            {
                BuildingId = original.BuildingId,
                HousingComplexId = original.HousingComplexId,
                ScopeType = original.ScopeType,
                ExpenseCategoryId = original.ExpenseCategoryId,
                ExpenseType = original.ExpenseType,
                PayerType = original.PayerType,
                Description = $"[TERS KAYIT / İPTAL] {original.Description} ({reason})",
                TotalAmount = -original.TotalAmount,
                Date = DateTime.Now,
                IsArchived = true,
                ArchivedAt = DateTime.UtcNow,
                ArchivedByUserId = actorUserId,
                ReversalOfExpenseId = original.Id
            };
            _context.BuildingExpenses.Add(reversal);
            await _context.SaveChangesAsync();

            var origDebts = await _context.UnitDebts.Where(d => d.BuildingExpenseId == original.Id && !d.IsArchived).ToListAsync();
            foreach (var d in origDebts)
            {
                d.IsArchived = true;
                _context.UnitDebts.Add(new UnitDebt
                {
                    BuildingUnitId = d.BuildingUnitId,
                    BuildingExpenseId = reversal.Id,
                    ResidencyPeriodId = d.ResidencyPeriodId,
                    PayerRole = d.PayerRole,
                    Amount = -d.Amount,
                    IsPaid = d.IsPaid,
                    PaymentDate = d.IsPaid ? DateTime.Now : null,
                    PaidBy = d.IsPaid ? "Ters Kayıt İadesi" : string.Empty,
                    IsArchived = true,
                    ReversalOfDebtId = d.Id
                });
            }

            WriteAuditLog(actorUserId, "EXPENSE_REVERSAL", "BuildingExpense", original.Id.ToString(), new
            {
                OriginalId = original.Id,
                ReversalId = reversal.Id,
                Amount = reversal.TotalAmount,
                Reason = reason
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }

        private void WriteAuditLog(string userId, string actionType, string entityName, string entityId, object payload)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = string.IsNullOrEmpty(userId) ? "system" : userId,
                ActionType = actionType,
                EntityName = entityName,
                EntityId = entityId,
                OldValues = "{}",
                NewValues = JsonSerializer.Serialize(payload),
                AffectedColumns = entityName,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
