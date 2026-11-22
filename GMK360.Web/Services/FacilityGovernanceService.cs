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

    public sealed class ExpenseDistributionPreviewItem
    {
        public int BuildingUnitId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        public string DoorNumber { get; set; } = string.Empty;
        public string Category { get; set; } = "Daire";
        public string RoomLayout { get; set; } = string.Empty;
        public decimal LandShare { get; set; }
        public double GrossSquareMeters { get; set; }
        public decimal DuesMultiplier { get; set; }
        public decimal ParticipationPercent { get; set; }
        public decimal EffectiveWeight { get; set; }
        public decimal OwnerAmount { get; set; }
        public decimal TenantAmount { get; set; }
        public decimal TotalShareAmount { get; set; }
        public string? OwnerName { get; set; }
        public string? TenantName { get; set; }
        public string StatusNote { get; set; } = string.Empty;
        public bool IsExempt => TotalShareAmount <= 0m;
    }

    public sealed class ExpenseDistributionPreviewResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ScopeLabel { get; set; } = string.Empty;
        public string DistributionRule { get; set; } = "Esit";
        public decimal TotalAmount { get; set; }
        public int ParticipatingUnitCount { get; set; }
        public int ExemptUnitCount { get; set; }
        public List<ExpenseDistributionPreviewItem> Items { get; set; } = new();
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

        public Task<ExpenseDistributionPreviewResult> PreviewExpenseDistributionAsync(
            int buildingId,
            decimal totalAmount,
            BuildingExpenseType expenseType,
            string distributionRule = "Esit",
            string expenseItemKey = "Genel",
            ExpenseScopeType scopeType = ExpenseScopeType.Building,
            decimal shopParticipationPercent = 100m,
            decimal? ownerSplitPercent = null)
        {
            return ComputeExpenseDistributionInternalAsync(
                buildingId,
                totalAmount,
                expenseType,
                distributionRule,
                expenseItemKey,
                scopeType,
                shopParticipationPercent,
                ownerSplitPercent);
        }

        private async Task<List<BuildingUnit>> LoadTargetLivingUnitsAsync(int buildingId, ExpenseScopeType scopeType)
        {
            List<int> targetBuildingIds;
            if (scopeType == ExpenseScopeType.HousingComplex)
            {
                var current = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == buildingId);
                int rootId = current?.ParentBuildingId ?? buildingId;
                targetBuildingIds = await _context.Buildings
                    .AsNoTracking()
                    .Where(b => b.Id == rootId || b.ParentBuildingId == rootId)
                    .Select(b => b.Id)
                    .ToListAsync();
                if (!targetBuildingIds.Any()) targetBuildingIds = new List<int> { buildingId };
            }
            else
            {
                targetBuildingIds = new List<int> { buildingId };
            }

            return await _context.BuildingUnits
                .Include(u => u.Building)
                .Where(u => targetBuildingIds.Contains(u.BuildingId)
                    && u.Category != "OrtakAlan_Blok"
                    && (u.Category == null || !u.Category.StartsWith("OrtakAlan")))
                .OrderBy(u => u.BuildingId)
                .ThenBy(u => u.FloorLevel)
                .ThenBy(u => u.DoorNumber)
                .ToListAsync();
        }

        private async Task<ExpenseDistributionPreviewResult> ComputeExpenseDistributionInternalAsync(
            int buildingId,
            decimal totalAmount,
            BuildingExpenseType expenseType,
            string distributionRule,
            string expenseItemKey,
            ExpenseScopeType scopeType,
            decimal shopParticipationPercent,
            decimal? ownerSplitPercent)
        {
            if (totalAmount <= 0)
            {
                return new ExpenseDistributionPreviewResult { Success = false, Message = "Gider tutarı sıfırdan büyük olmalıdır." };
            }

            var units = await LoadTargetLivingUnitsAsync(buildingId, scopeType);
            if (!units.Any())
            {
                return new ExpenseDistributionPreviewResult { Success = false, Message = "Dağıtılacak aktif daire veya dükkan bulunamadı." };
            }

            string normalizedRule = string.IsNullOrWhiteSpace(distributionRule) ? "Esit" : distributionRule.Trim();
            string normalizedItem = string.IsNullOrWhiteSpace(expenseItemKey) ? "Genel" : expenseItemKey.Trim();
            bool isCapital = expenseType == BuildingExpenseType.CapitalExpenditure;
            decimal clampedShopPct = Math.Max(0m, shopParticipationPercent);

            // Her birim için masraf kalemine bağlı muafiyet / katılım yüzdesi
            var unitFactors = new Dictionary<int, (decimal Factor, string Note)>();
            foreach (var u in units)
            {
                bool isShop = string.Equals(u.Category, "Dukkan", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(u.Category, "Dükkan", StringComparison.OrdinalIgnoreCase);

                decimal factor = 1.00m;
                string note = "Standart Katılım (%100)";

                if (string.Equals(normalizedItem, "Asansor", StringComparison.OrdinalIgnoreCase) && u.IsExemptFromElevator)
                {
                    factor = 0m;
                    note = "Asansör Kaleminden Muaf (%0)";
                }
                else if (string.Equals(normalizedItem, "Merdiven", StringComparison.OrdinalIgnoreCase) && u.IsExemptFromStairwell)
                {
                    factor = 0m;
                    note = "Merdiven/Aydınlatmadan Muaf (%0)";
                }
                else if (string.Equals(normalizedItem, "CopTemizlik", StringComparison.OrdinalIgnoreCase))
                {
                    if (u.CleaningParticipationPercent != 100m)
                    {
                        factor = Math.Max(0m, u.CleaningParticipationPercent) / 100m;
                        note = factor == 0m
                            ? "Çöp/Temizlik Muaf (%0)"
                            : $"Birim Çöp/Temizlik Oranı (%{u.CleaningParticipationPercent:0.##})";
                    }
                    else if (isShop && clampedShopPct != 100m)
                    {
                        factor = clampedShopPct / 100m;
                        note = factor == 0m
                            ? "Dükkan Muaf (%0)"
                            : (factor > 1m ? $"Dükkan Yoğun Çöp/Temizlik (%{clampedShopPct:0.##})" : $"Dükkan İndirimli (%{clampedShopPct:0.##})");
                    }
                }
                else if (isShop && clampedShopPct != 100m)
                {
                    factor = clampedShopPct / 100m;
                    note = factor == 0m
                        ? $"Dükkan Muaf ({normalizedItem} %0)"
                        : (factor > 1m ? $"Dükkan Artışlı Pay (%{clampedShopPct:0.##})" : $"Dükkan İndirimli Pay (%{clampedShopPct:0.##})");
                }

                unitFactors[u.Id] = (factor, note);
            }

            // Sessiz fallback yerine eksik arsa payı / m² kontrolü (sadece muaf olmayan birimlerde)
            var participatingUnits = units.Where(u => unitFactors[u.Id].Factor > 0m).ToList();
            if (!participatingUnits.Any())
            {
                return new ExpenseDistributionPreviewResult
                {
                    Success = false,
                    Message = "Seçilen kural ve muafiyetlere göre katılımcı birim kalmadığı için dağıtım yapılamıyor."
                };
            }

            if (string.Equals(normalizedRule, "ArsaPayi", StringComparison.OrdinalIgnoreCase))
            {
                var missing = participatingUnits.Where(u => u.LandShare <= 0m).Select(u => u.DoorNumber).Take(6).ToList();
                if (missing.Any())
                {
                    return new ExpenseDistributionPreviewResult
                    {
                        Success = false,
                        Message = $"Arsa Payı dağıtımı için önce şu bölümlere Arsa Payı girilmelidir (Sakinler / Daire düzenleden): {string.Join(", ", missing)}"
                    };
                }
            }
            else if (string.Equals(normalizedRule, "Metrekare", StringComparison.OrdinalIgnoreCase))
            {
                var missing = participatingUnits.Where(u => (u.GrossSquareMeters ?? 0) <= 0).Select(u => u.DoorNumber).Take(6).ToList();
                if (missing.Any())
                {
                    return new ExpenseDistributionPreviewResult
                    {
                        Success = false,
                        Message = $"Metrekare dağıtımı için şu bölümlere Brüt m² girilmelidir: {string.Join(", ", missing)}"
                    };
                }
            }

            // Efektif ağırlıklar
            var weights = new Dictionary<int, decimal>();
            foreach (var u in units)
            {
                var (factor, _) = unitFactors[u.Id];
                decimal baseWeight = normalizedRule.ToLowerInvariant() switch
                {
                    "arsapayi" => u.LandShare,
                    "metrekare" => (decimal)(u.GrossSquareMeters ?? 0),
                    "katsayi" => u.DuesMultiplier > 0m ? u.DuesMultiplier : 1.00m,
                    _ => 1.00m
                };
                weights[u.Id] = Math.Max(0m, baseWeight * factor);
            }

            decimal totalWeight = weights.Values.Sum();
            if (totalWeight <= 0m)
            {
                return new ExpenseDistributionPreviewResult { Success = false, Message = "Toplam dağıtım ağırlığı sıfır çıktı." };
            }

            // Kuruş-dengeli (Remainder Allocation) paylaşım
            var exactMap = new Dictionary<int, decimal>();
            var shareMap = new Dictionary<int, decimal>();
            foreach (var u in units)
            {
                decimal w = weights[u.Id];
                decimal exact = w > 0m ? totalAmount * (w / totalWeight) : 0m;
                exactMap[u.Id] = exact;
                shareMap[u.Id] = Math.Round(exact, 2, MidpointRounding.AwayFromZero);
            }

            decimal diff = totalAmount - shareMap.Values.Sum();
            int remainderCents = (int)Math.Round(diff * 100m, 0, MidpointRounding.AwayFromZero);
            if (remainderCents != 0)
            {
                int stepDir = remainderCents > 0 ? 1 : -1;
                var candidates = units
                    .Where(u => weights[u.Id] > 0m)
                    .OrderByDescending(u => stepDir > 0
                        ? (exactMap[u.Id] - shareMap[u.Id])
                        : (shareMap[u.Id] - exactMap[u.Id]))
                    .ThenByDescending(u => weights[u.Id])
                    .ToList();

                int idx = 0;
                while (remainderCents != 0 && candidates.Any())
                {
                    var cand = candidates[idx % candidates.Count];
                    decimal nextVal = shareMap[cand.Id] + (stepDir * 0.01m);
                    if (nextVal >= 0m)
                    {
                        shareMap[cand.Id] = nextVal;
                        remainderCents -= stepDir;
                    }
                    idx++;
                    if (idx > candidates.Count * 10) break;
                }
            }

            decimal? effectiveOwnerPct = ownerSplitPercent.HasValue
                ? Math.Clamp(ownerSplitPercent.Value, 0m, 100m)
                : (isCapital ? 100m : (decimal?)null);

            var previewItems = new List<ExpenseDistributionPreviewItem>();
            foreach (var u in units)
            {
                decimal unitShare = shareMap[u.Id];
                var (factor, baseNote) = unitFactors[u.Id];
                bool hasTenant = !u.IsEmpty && !string.IsNullOrWhiteSpace(u.TenantName);

                decimal ownerAmt = 0m;
                decimal tenantAmt = 0m;
                string payerNote;

                if (unitShare <= 0m)
                {
                    payerNote = baseNote;
                }
                else if (effectiveOwnerPct.HasValue)
                {
                    if (effectiveOwnerPct.Value >= 100m || !hasTenant)
                    {
                        ownerAmt = unitShare;
                        payerNote = effectiveOwnerPct.Value >= 100m && hasTenant
                            ? "Kiracı Muaf → Malik %100"
                            : (!hasTenant ? "Malik (Kiracı Yok)" : baseNote);
                    }
                    else if (effectiveOwnerPct.Value <= 0m)
                    {
                        tenantAmt = unitShare;
                        payerNote = "Kiracı %100";
                    }
                    else
                    {
                        ownerAmt = Math.Round(unitShare * (effectiveOwnerPct.Value / 100m), 2, MidpointRounding.AwayFromZero);
                        tenantAmt = unitShare - ownerAmt;
                        payerNote = $"Karma %{effectiveOwnerPct.Value:0.##} Malik / %{(100m - effectiveOwnerPct.Value):0.##} Kiracı";
                    }
                }
                else
                {
                    if (hasTenant)
                    {
                        tenantAmt = unitShare;
                        payerNote = "Kiracı (İşletme)";
                    }
                    else
                    {
                        ownerAmt = unitShare;
                        payerNote = "Malik (Oturan / Boş)";
                    }
                }

                string combinedNote = unitShare <= 0m || baseNote == "Standart Katılım (%100)"
                    ? payerNote
                    : $"{baseNote} • {payerNote}";

                previewItems.Add(new ExpenseDistributionPreviewItem
                {
                    BuildingUnitId = u.Id,
                    BuildingName = u.Building?.Name ?? "-",
                    DoorNumber = u.DoorNumber,
                    Category = u.Category ?? "Daire",
                    RoomLayout = u.RoomLayout ?? (string.Equals(u.Category, "Dukkan", StringComparison.OrdinalIgnoreCase) ? "Dükkan" : "Daire"),
                    LandShare = u.LandShare,
                    GrossSquareMeters = u.GrossSquareMeters ?? 0,
                    DuesMultiplier = u.DuesMultiplier > 0m ? u.DuesMultiplier : 1.00m,
                    ParticipationPercent = Math.Round(factor * 100m, 2),
                    EffectiveWeight = Math.Round(weights[u.Id], 4),
                    OwnerAmount = ownerAmt,
                    TenantAmount = tenantAmt,
                    TotalShareAmount = unitShare,
                    OwnerName = u.OwnerName,
                    TenantName = u.TenantName,
                    StatusNote = combinedNote
                });
            }

            return new ExpenseDistributionPreviewResult
            {
                Success = true,
                Message = "Dağıtım hesaplaması hazır.",
                ScopeLabel = scopeType == ExpenseScopeType.HousingComplex ? "Tüm Site / Bloklar" : (units.FirstOrDefault()?.Building?.Name ?? "Seçili Blok"),
                DistributionRule = normalizedRule,
                TotalAmount = totalAmount,
                ParticipatingUnitCount = previewItems.Count(x => x.TotalShareAmount > 0m),
                ExemptUnitCount = previewItems.Count(x => x.TotalShareAmount <= 0m),
                Items = previewItems
            };
        }

        private async Task PersistUnitDebtsFromPreviewAsync(
            int buildingId,
            ExpenseScopeType scopeType,
            BuildingExpense expense,
            ExpenseDistributionPreviewResult preview)
        {
            var unitLookup = (await LoadTargetLivingUnitsAsync(buildingId, scopeType)).ToDictionary(u => u.Id);
            foreach (var item in preview.Items.Where(i => i.TotalShareAmount > 0m))
            {
                if (!unitLookup.TryGetValue(item.BuildingUnitId, out var unit)) continue;

                if (item.OwnerAmount > 0m)
                {
                    var ownerPeriod = await EnsureActivePeriodFromUnitAsync(unit, "Owner");
                    _context.UnitDebts.Add(new UnitDebt
                    {
                        BuildingUnitId = unit.Id,
                        BuildingExpenseId = expense.Id,
                        ResidencyPeriodId = ownerPeriod?.Id,
                        PayerRole = "Owner",
                        Amount = item.OwnerAmount,
                        IsPaid = false
                    });
                }

                if (item.TenantAmount > 0m)
                {
                    var tenantPeriod = await EnsureActivePeriodFromUnitAsync(unit, "Tenant");
                    _context.UnitDebts.Add(new UnitDebt
                    {
                        BuildingUnitId = unit.Id,
                        BuildingExpenseId = expense.Id,
                        ResidencyPeriodId = tenantPeriod?.Id,
                        PayerRole = "Tenant",
                        Amount = item.TenantAmount,
                        IsPaid = false
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        public static string ResolveExpenseGroup(BuildingExpenseType expenseType, string? expenseItemKey, string? explicitGroup = null)
        {
            if (!string.IsNullOrWhiteSpace(explicitGroup))
            {
                return explicitGroup.Trim() switch
                {
                    "Personel" => "Personel",
                    "FaturaAbonelik" => "FaturaAbonelik",
                    "BakimOnarim" => "BakimOnarim",
                    "DemirbasYatirim" => "DemirbasYatirim",
                    "YonetimIdari" => "YonetimIdari",
                    _ => explicitGroup.Trim()
                };
            }

            if (expenseType == BuildingExpenseType.CapitalExpenditure
                || string.Equals(expenseItemKey, "Demirbas", StringComparison.OrdinalIgnoreCase))
            {
                return "DemirbasYatirim";
            }

            return (expenseItemKey ?? "Genel").Trim().ToLowerInvariant() switch
            {
                "personel" => "Personel",
                "isinma" => "FaturaAbonelik",
                "asansor" or "merdiven" or "copTemizlik" or "coptemizlik" => "BakimOnarim",
                _ => "FaturaAbonelik"
            };
        }

        public async Task<GovernanceActionResult> DistributeExpenseAsync(
            int buildingId,
            string description,
            decimal totalAmount,
            BuildingExpenseType expenseType,
            IFormFile? invoiceFile,
            string actorUserId,
            string distributionRule = "Esit",
            string expenseItemKey = "Genel",
            ExpenseScopeType scopeType = ExpenseScopeType.Building,
            decimal shopParticipationPercent = 100m,
            decimal? ownerSplitPercent = null,
            string? expenseGroup = null)
        {
            var preview = await ComputeExpenseDistributionInternalAsync(
                buildingId,
                totalAmount,
                expenseType,
                distributionRule,
                expenseItemKey,
                scopeType,
                shopParticipationPercent,
                ownerSplitPercent);

            if (!preview.Success)
                return new GovernanceActionResult { Success = false, Message = preview.Message };

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
            string payerType = ownerSplitPercent.HasValue
                ? (ownerSplitPercent.Value >= 100m ? "Owner" : (ownerSplitPercent.Value <= 0m ? "Tenant" : "Split"))
                : (isCapital ? "Owner" : "Tenant");

            string resolvedGroup = ResolveExpenseGroup(expenseType, expenseItemKey, expenseGroup);

            var expense = new BuildingExpense
            {
                BuildingId = buildingId,
                ScopeType = scopeType,
                ExpenseType = expenseType,
                ExpenseGroup = resolvedGroup,
                PayerType = payerType,
                ExpenseItemKey = string.IsNullOrWhiteSpace(expenseItemKey) ? "Genel" : expenseItemKey.Trim(),
                DistributionRule = preview.DistributionRule,
                ShopParticipationPercent = Math.Max(0m, shopParticipationPercent),
                OwnerSplitPercent = ownerSplitPercent,
                Description = description.Trim(),
                TotalAmount = totalAmount,
                InvoiceDocumentUrl = invoiceUrl,
                Date = DateTime.Now
            };
            _context.BuildingExpenses.Add(expense);
            await _context.SaveChangesAsync();

            await PersistUnitDebtsFromPreviewAsync(buildingId, scopeType, expense, preview);

            WriteAuditLog(actorUserId, "EXPENSE_CREATED", "BuildingExpense", expense.Id.ToString(), new
            {
                buildingId,
                expense.TotalAmount,
                ExpenseType = expense.ExpenseType.ToString(),
                ScopeType = scopeType.ToString(),
                expense.ExpenseItemKey,
                expense.DistributionRule,
                expense.ShopParticipationPercent,
                expense.OwnerSplitPercent,
                Participating = preview.ParticipatingUnitCount,
                Exempt = preview.ExemptUnitCount,
                expense.InvoiceDocumentUrl
            });
            await _context.SaveChangesAsync();

            string exemptInfo = preview.ExemptUnitCount > 0 ? $" ({preview.ExemptUnitCount} muaf bölüm hariç tutuldu)" : "";
            return new GovernanceActionResult
            {
                Success = true,
                CreatedExpenseId = expense.Id,
                Message = $"'{description.Trim()}' ({totalAmount:N2} ₺) {preview.DistributionRule} kuralıyla {preview.ParticipatingUnitCount} bölüme exact kuruşlu paylaştırıldı{exemptInfo}."
            };
        }

        public async Task<ExpenseDistributionPreviewResult> PreviewFixedUnitDuesAsync(
            int buildingId,
            int year,
            int month,
            decimal defaultBaseDues = 0m,
            ExpenseScopeType scopeType = ExpenseScopeType.Building,
            decimal? ownerSplitPercent = null)
        {
            var units = await LoadTargetLivingUnitsAsync(buildingId, scopeType);
            if (!units.Any())
                return new ExpenseDistributionPreviewResult { Success = false, Message = "Aktif bağımsız bölüm bulunamadı." };

            decimal ownerPct = ownerSplitPercent.HasValue ? Math.Clamp(ownerSplitPercent.Value, 0m, 100m) : 0m;
            var items = new List<ExpenseDistributionPreviewItem>();
            decimal grandTotal = 0m;

            foreach (var u in units)
            {
                decimal mult = u.DuesMultiplier > 0m ? u.DuesMultiplier : 1.00m;
                decimal share = u.FixedMonthlyDues.HasValue && u.FixedMonthlyDues.Value >= 0m
                    ? Math.Round(u.FixedMonthlyDues.Value, 2, MidpointRounding.AwayFromZero)
                    : Math.Round(Math.Max(0m, defaultBaseDues) * mult, 2, MidpointRounding.AwayFromZero);

                bool hasTenant = !u.IsEmpty && !string.IsNullOrWhiteSpace(u.TenantName);
                decimal ownerAmt = 0m;
                decimal tenantAmt = 0m;
                string note;

                if (share <= 0m)
                {
                    note = "Sabit Aidat = 0 ₺ (Muaf / Tanımsız)";
                }
                else if (!hasTenant || ownerPct >= 100m)
                {
                    ownerAmt = share;
                    note = hasTenant && ownerPct >= 100m ? "Kiracı Muaf → Malik %100" : "Malik (Oturan / Boş)";
                }
                else if (ownerPct <= 0m)
                {
                    tenantAmt = share;
                    note = u.FixedMonthlyDues.HasValue ? "Tanımlı Sabit Aidat (Kiracı)" : $"Baz × {mult:0.##}x (Kiracı)";
                }
                else
                {
                    ownerAmt = Math.Round(share * (ownerPct / 100m), 2, MidpointRounding.AwayFromZero);
                    tenantAmt = share - ownerAmt;
                    note = $"%{ownerPct:0.##} Malik / %{(100m - ownerPct):0.##} Kiracı";
                }

                grandTotal += share;
                items.Add(new ExpenseDistributionPreviewItem
                {
                    BuildingUnitId = u.Id,
                    BuildingName = u.Building?.Name ?? "-",
                    DoorNumber = u.DoorNumber,
                    Category = u.Category ?? "Daire",
                    RoomLayout = u.RoomLayout ?? "Daire",
                    LandShare = u.LandShare,
                    GrossSquareMeters = u.GrossSquareMeters ?? 0,
                    DuesMultiplier = mult,
                    ParticipationPercent = share > 0m ? 100m : 0m,
                    EffectiveWeight = mult,
                    OwnerAmount = ownerAmt,
                    TenantAmount = tenantAmt,
                    TotalShareAmount = share,
                    OwnerName = u.OwnerName,
                    TenantName = u.TenantName,
                    StatusNote = note
                });
            }

            if (grandTotal <= 0m)
            {
                return new ExpenseDistributionPreviewResult
                {
                    Success = false,
                    Message = "Toplam aidat 0 ₺ çıktı. Bölümlere Sabit Aylık Aidat tanımlayın veya Baz Aidat Tutarı girin."
                };
            }

            return new ExpenseDistributionPreviewResult
            {
                Success = true,
                Message = $"{year}-{month:00} aylık sabit aidat önizlemesi hazır.",
                ScopeLabel = scopeType == ExpenseScopeType.HousingComplex ? "Tüm Site / Bloklar" : (units.FirstOrDefault()?.Building?.Name ?? "Seçili Blok"),
                DistributionRule = "SabitAidat_Katsayi",
                TotalAmount = grandTotal,
                ParticipatingUnitCount = items.Count(i => i.TotalShareAmount > 0m),
                ExemptUnitCount = items.Count(i => i.TotalShareAmount <= 0m),
                Items = items
            };
        }

        public async Task<GovernanceActionResult> DistributeFixedUnitDuesAsync(
            int buildingId,
            int year,
            int month,
            decimal defaultBaseDues,
            ExpenseScopeType scopeType,
            decimal? ownerSplitPercent,
            string actorUserId,
            string? customDescription = null)
        {
            var preview = await PreviewFixedUnitDuesAsync(buildingId, year, month, defaultBaseDues, scopeType, ownerSplitPercent);
            if (!preview.Success)
                return new GovernanceActionResult { Success = false, Message = preview.Message };

            string desc = string.IsNullOrWhiteSpace(customDescription)
                ? $"{year}-{month:00} Aylık Aidat Tahakkuku"
                : customDescription.Trim();

            var expense = new BuildingExpense
            {
                BuildingId = buildingId,
                ScopeType = scopeType,
                ExpenseType = ownerSplitPercent.HasValue && ownerSplitPercent.Value >= 100m
                    ? BuildingExpenseType.CapitalExpenditure
                    : BuildingExpenseType.Operational,
                PayerType = ownerSplitPercent.HasValue && ownerSplitPercent.Value >= 100m ? "Owner" : "Tenant",
                ExpenseItemKey = "AidatTahakkuku",
                DistributionRule = "Katsayi",
                ShopParticipationPercent = 100m,
                OwnerSplitPercent = ownerSplitPercent,
                Description = desc,
                TotalAmount = preview.TotalAmount,
                Date = DateTime.Now
            };
            _context.BuildingExpenses.Add(expense);
            await _context.SaveChangesAsync();

            await PersistUnitDebtsFromPreviewAsync(buildingId, scopeType, expense, preview);

            WriteAuditLog(actorUserId, "MONTHLY_DUES_POSTED", "BuildingExpense", expense.Id.ToString(), new
            {
                buildingId,
                year,
                month,
                expense.TotalAmount,
                Participating = preview.ParticipatingUnitCount
            });
            await _context.SaveChangesAsync();

            return new GovernanceActionResult
            {
                Success = true,
                CreatedExpenseId = expense.Id,
                Message = $"✅ {desc} başarıyla {preview.ParticipatingUnitCount} bölüme borçlandırıldı (Toplam: {preview.TotalAmount:N2} ₺)."
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
