using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Services
{
    public sealed class BuildingMonthlyBudgetSummary
    {
        public int BuildingId { get; set; }
        public int ActiveUnitCount { get; set; }
        public decimal MonthlyStaffOperatingCost { get; set; }
        public decimal MonthlyFixedOperatingCost { get; set; }
        public decimal MonthlyFixedCapitalCost { get; set; }
        public decimal MonthlySeveranceFundTotal { get; set; }
        public decimal TotalMonthlyOperatingBudget => MonthlyStaffOperatingCost + MonthlyFixedOperatingCost;
        public decimal TotalMonthlyCapitalBudget => MonthlyFixedCapitalCost + MonthlySeveranceFundTotal;
        public decimal EstimatedMinOperatingDuesPerUnit =>
            ActiveUnitCount > 0 ? Math.Round(TotalMonthlyOperatingBudget / ActiveUnitCount, 2) : 0m;
        public decimal EstimatedSeverancePerOwnerUnit =>
            ActiveUnitCount > 0 ? Math.Round(TotalMonthlyCapitalBudget / ActiveUnitCount, 2) : 0m;
    }

    public sealed class LeaveActionResult
    {
        public bool Success { get; set; }
        public string? OverlapWarning { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Bina sabit gider bütçesi, personel SGK &amp; Kıdem Fonu karşılığı, yıllık izin çakışma uyarısı
    /// ve resmi/dini bayram çift yevmiye aylık puantaj motoru.
    /// </summary>
    public class FacilityPayrollAndBudgetService
    {
        private readonly ApplicationDbContext _context;
        private readonly FacilityGovernanceService _governanceService;

        public FacilityPayrollAndBudgetService(
            ApplicationDbContext context,
            FacilityGovernanceService governanceService)
        {
            _context = context;
            _governanceService = governanceService;
        }

        public async Task<BuildingMonthlyBudgetSummary> CalculateMonthlyBudgetSummaryAsync(int buildingId)
        {
            int activeUnitCount = await _context.BuildingUnits.CountAsync(u =>
                u.BuildingId == buildingId
                && u.Category != "OrtakAlan_Blok"
                && (u.Category == null || !u.Category.StartsWith("OrtakAlan")));

            var staffs = await _context.BuildingStaffs
                .AsNoTracking()
                .Where(s => s.BuildingId == buildingId && s.IsActive && !s.IsDeleted)
                .ToListAsync();

            var fixedExpenses = await _context.BuildingFixedExpenses
                .AsNoTracking()
                .Where(f => f.BuildingId == buildingId && f.IsActive && f.IsAutoRecurring && !f.IsDeleted)
                .ToListAsync();

            return new BuildingMonthlyBudgetSummary
            {
                BuildingId = buildingId,
                ActiveUnitCount = activeUnitCount,
                MonthlyStaffOperatingCost = staffs.Sum(s => s.TotalMonthlyOperatingCost),
                MonthlySeveranceFundTotal = staffs.Sum(s => s.MonthlySeveranceFund),
                MonthlyFixedOperatingCost = fixedExpenses.Where(f => f.ExpenseScope != "Demirbas").Sum(f => f.MonthlyAmount),
                MonthlyFixedCapitalCost = fixedExpenses.Where(f => f.ExpenseScope == "Demirbas").Sum(f => f.MonthlyAmount)
            };
        }

        public async Task<string?> CheckLeaveOverlapAsync(int buildingId, int staffId, string roleType, DateTime startDate, DateTime endDate)
        {
            var sameRoleStaff = await _context.BuildingStaffs
                .AsNoTracking()
                .Where(s => s.BuildingId == buildingId && s.IsActive && !s.IsDeleted && s.Id != staffId)
                .ToListAsync();

            if (!sameRoleStaff.Any()) return null;

            var sameRoleIds = sameRoleStaff
                .Where(s => string.Equals(s.RoleType, roleType, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id)
                .ToList();

            if (!sameRoleIds.Any()) return null;

            var overlapping = await _context.BuildingStaffLeaves
                .Include(l => l.Staff)
                .AsNoTracking()
                .Where(l => sameRoleIds.Contains(l.BuildingStaffId)
                    && !l.IsDeleted
                    && l.Status != "Iptal"
                    && l.StartDate.Date <= endDate.Date
                    && l.EndDate.Date >= startDate.Date)
                .ToListAsync();

            if (!overlapping.Any()) return null;

            var names = string.Join(", ", overlapping.Select(o => $"{o.Staff.FullName} ({o.StartDate:dd.MM}-{o.EndDate:dd.MM})").Distinct());
            return $"Çakışma Uyarısı: Aynı tarihlerde '{roleType}' kadrosundan {names} de izinde göründüğü için vardiyada personel kalmayabilir!";
        }

        public async Task<LeaveActionResult> PlanStaffLeaveAsync(
            int staffId,
            string leaveType,
            DateTime startDate,
            DateTime endDate,
            decimal replacementCost,
            string? notes)
        {
            var staff = await _context.BuildingStaffs.FirstOrDefaultAsync(s => s.Id == staffId && !s.IsDeleted);
            if (staff == null)
                return new LeaveActionResult { Success = false, Message = "Personel kaydı bulunamadı." };
            if (endDate.Date < startDate.Date)
                return new LeaveActionResult { Success = false, Message = "Bitiş tarihi başlangıç tarihinden önce olamaz." };

            int totalDays = (int)(endDate.Date - startDate.Date).TotalDays + 1;
            string? overlapWarning = await CheckLeaveOverlapAsync(staff.BuildingId, staff.Id, staff.RoleType, startDate, endDate);

            var leave = new BuildingStaffLeave
            {
                BuildingStaffId = staff.Id,
                LeaveYear = startDate.Year,
                LeaveType = string.IsNullOrWhiteSpace(leaveType) ? "YillikIzin" : leaveType.Trim(),
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                TotalDays = totalDays,
                Status = "Onaylandi",
                ReplacementCost = Math.Max(0m, replacementCost),
                Notes = notes?.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _context.BuildingStaffLeaves.Add(leave);
            await _context.SaveChangesAsync();

            return new LeaveActionResult
            {
                Success = true,
                OverlapWarning = overlapWarning,
                Message = overlapWarning != null
                    ? $"İzin kaydedildi ({totalDays} gün). {overlapWarning}"
                    : $"{staff.FullName} için {totalDays} günlük izin planlandı."
            };
        }

        public async Task<BuildingStaffMonthlyPayroll?> GenerateOrUpdateMonthlyPayrollAsync(
            int staffId,
            int year,
            int month,
            decimal holidayDaysWorked,
            int unpaidLeaveDays,
            decimal? overrideReplacementCost = null)
        {
            var staff = await _context.BuildingStaffs.FirstOrDefaultAsync(s => s.Id == staffId && !s.IsDeleted);
            if (staff == null) return null;

            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            decimal plannedReplacement = await _context.BuildingStaffLeaves
                .Where(l => l.BuildingStaffId == staffId
                    && !l.IsDeleted
                    && l.StartDate <= monthEnd
                    && l.EndDate >= monthStart)
                .SumAsync(l => (decimal?)l.ReplacementCost) ?? 0m;

            var payroll = await _context.BuildingStaffMonthlyPayrolls
                .FirstOrDefaultAsync(p => p.BuildingStaffId == staffId && p.Year == year && p.Month == month && !p.IsDeleted);

            if (payroll == null)
            {
                payroll = new BuildingStaffMonthlyPayroll
                {
                    BuildingStaffId = staff.Id,
                    Year = year,
                    Month = month,
                    CreatedAt = DateTime.UtcNow
                };
                _context.BuildingStaffMonthlyPayrolls.Add(payroll);
            }

            payroll.BaseNetSalary = staff.NetSalary;
            payroll.BaseSgkEmployerShare = staff.SgkEmployerShare;
            payroll.BaseIncomeAndStampTax = staff.IncomeAndStampTax;
            payroll.BaseFringeBenefits = staff.FringeBenefits;
            payroll.MonthlySeveranceFund = staff.MonthlySeveranceFund;
            payroll.HolidayDaysWorked = Math.Max(0m, holidayDaysWorked);
            payroll.UnpaidLeaveDays = Math.Max(0, unpaidLeaveDays);
            payroll.ReplacementStaffCost = overrideReplacementCost ?? plannedReplacement;
            payroll.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return payroll;
        }

        public async Task<GovernanceActionResult> MarkPayrollPaidAndPostLedgersAsync(
            int payrollId,
            string paymentKind,
            int? bankAccountId,
            bool distributeOperatingToUnits,
            bool postSeveranceFundToOwners,
            string actorUserId)
        {
            var payroll = await _context.BuildingStaffMonthlyPayrolls
                .Include(p => p.Staff)
                .FirstOrDefaultAsync(p => p.Id == payrollId && !p.IsDeleted);

            if (payroll == null || payroll.Staff == null)
                return new GovernanceActionResult { Success = false, Message = "Bordro kaydı bulunamadı." };

            decimal bankDeduction = 0m;
            if (string.Equals(paymentKind, "Salary", StringComparison.OrdinalIgnoreCase))
            {
                if (!payroll.IsSalaryPaid)
                {
                    payroll.IsSalaryPaid = true;
                    payroll.SalaryPaidAt = DateTime.Now;
                    bankDeduction += payroll.NetPayableToStaff + payroll.BaseFringeBenefits;
                }
            }
            else if (string.Equals(paymentKind, "Sgk", StringComparison.OrdinalIgnoreCase))
            {
                if (!payroll.IsSgkPaid)
                {
                    payroll.IsSgkPaid = true;
                    payroll.SgkPaidAt = DateTime.Now;
                    bankDeduction += payroll.BaseSgkEmployerShare + payroll.BaseIncomeAndStampTax;
                }
            }
            else
            {
                if (!payroll.IsSalaryPaid)
                {
                    payroll.IsSalaryPaid = true;
                    payroll.SalaryPaidAt = DateTime.Now;
                    bankDeduction += payroll.NetPayableToStaff + payroll.BaseFringeBenefits;
                }
                if (!payroll.IsSgkPaid)
                {
                    payroll.IsSgkPaid = true;
                    payroll.SgkPaidAt = DateTime.Now;
                    bankDeduction += payroll.BaseSgkEmployerShare + payroll.BaseIncomeAndStampTax;
                }
            }

            payroll.IsPaid = payroll.IsSalaryPaid && payroll.IsSgkPaid;
            if (payroll.IsPaid) payroll.PaymentDate ??= DateTime.Now;
            if (bankAccountId.HasValue) payroll.PaidFromBankAccountId = bankAccountId.Value;

            if (bankAccountId.HasValue && bankDeduction > 0)
            {
                var acc = await _context.BankAccounts.FirstOrDefaultAsync(a => a.Id == bankAccountId.Value && a.OwnerId == payroll.Staff.BuildingId);
                if (acc != null) acc.CurrentBalance -= bankDeduction;
            }
            await _context.SaveChangesAsync();

            if (distributeOperatingToUnits && !payroll.OperatingExpenseId.HasValue && payroll.TotalSiteCostForMonth > 0)
            {
                string holidayNote = payroll.HolidayDaysWorked > 0
                    ? $" (+{payroll.HolidayDaysWorked:0.#} gün bayram çift yevmiyesi: {payroll.HolidayExtraPay:N2} ₺)"
                    : "";
                var opRes = await _governanceService.DistributeExpenseAsync(
                    payroll.Staff.BuildingId,
                    $"Personel İşletme Maliyeti ({payroll.Year}-{payroll.Month:00}) - {payroll.Staff.FullName}{holidayNote}",
                    payroll.TotalSiteCostForMonth,
                    BuildingExpenseType.Operational,
                    null,
                    actorUserId,
                    "Esit",
                    "Personel",
                    ExpenseScopeType.Building,
                    100m,
                    null,
                    "Personel");
                payroll.OperatingExpenseId = opRes.CreatedExpenseId;
                await _context.SaveChangesAsync();
            }

            if (postSeveranceFundToOwners && !payroll.SeveranceExpenseId.HasValue && payroll.MonthlySeveranceFund > 0)
            {
                var sevRes = await _governanceService.DistributeExpenseAsync(
                    payroll.Staff.BuildingId,
                    $"Aylık Kıdem Tazminatı Fonu ({payroll.Year}-{payroll.Month:00}) - {payroll.Staff.FullName} (Malik Demirbaş Fonu)",
                    payroll.MonthlySeveranceFund,
                    BuildingExpenseType.CapitalExpenditure,
                    null,
                    actorUserId,
                    "Esit",
                    "Personel",
                    ExpenseScopeType.Building,
                    100m,
                    100m,
                    "Personel");
                payroll.SeveranceExpenseId = sevRes.CreatedExpenseId;
                await _context.SaveChangesAsync();
            }

            return new GovernanceActionResult
            {
                Success = true,
                Message = $"{payroll.Staff.FullName} ({payroll.Year}-{payroll.Month:00}) ödeme kaydı işlendi (Maaş: {(payroll.IsSalaryPaid ? "Ödendi" : "Bekliyor")}, SGK: {(payroll.IsSgkPaid ? "Ödendi" : "Bekliyor")})."
            };
        }

        public async Task<GovernanceActionResult> DistributeFixedExpenseAsync(int fixedExpenseId, int year, int month, string actorUserId)
        {
            var fixedExp = await _context.BuildingFixedExpenses.FirstOrDefaultAsync(f => f.Id == fixedExpenseId && !f.IsDeleted);
            if (fixedExp == null)
                return new GovernanceActionResult { Success = false, Message = "Sabit gider tanımı bulunamadı." };

            var expenseType = string.Equals(fixedExp.ExpenseScope, "Demirbas", StringComparison.OrdinalIgnoreCase)
                ? BuildingExpenseType.CapitalExpenditure
                : BuildingExpenseType.Operational;

            return await _governanceService.DistributeExpenseAsync(
                fixedExp.BuildingId,
                $"{fixedExp.Title} ({year}-{month:00} Sabit Bütçe)",
                fixedExp.MonthlyAmount,
                expenseType,
                null,
                actorUserId,
                fixedExp.DistributionRule,
                fixedExp.ExpenseItemKey,
                fixedExp.ScopeType,
                fixedExp.ShopParticipationPercent,
                fixedExp.OwnerSplitPercent,
                fixedExp.ExpenseGroup);
        }

        public async Task<ExpenseDistributionPreviewResult> PreviewAllRecurringBudgetAsync(int buildingId, int year, int month, bool includeStaffCosts)
        {
            var fixedItems = await _context.BuildingFixedExpenses
                .AsNoTracking()
                .Where(f => f.BuildingId == buildingId && f.IsActive && f.IsAutoRecurring && !f.IsDeleted && f.MonthlyAmount > 0m)
                .OrderBy(f => f.DueDayOfMonth)
                .ToListAsync();

            var activeStaff = includeStaffCosts
                ? await _context.BuildingStaffs.AsNoTracking().Where(s => s.BuildingId == buildingId && s.IsActive && !s.IsDeleted).ToListAsync()
                : new List<BuildingStaff>();

            if (!fixedItems.Any() && !activeStaff.Any())
            {
                return new ExpenseDistributionPreviewResult
                {
                    Success = false,
                    Message = "Bu bina için karma dağıtımda kullanılacak aktif sabit gider veya personel kaydı bulunamadı."
                };
            }

            var subPreviews = new List<(string Title, ExpenseDistributionPreviewResult Result)>();

            foreach (var fx in fixedItems)
            {
                var expenseType = string.Equals(fx.ExpenseScope, "Demirbas", StringComparison.OrdinalIgnoreCase)
                    ? BuildingExpenseType.CapitalExpenditure
                    : BuildingExpenseType.Operational;

                var prev = await _governanceService.PreviewExpenseDistributionAsync(
                    fx.BuildingId,
                    fx.MonthlyAmount,
                    expenseType,
                    fx.DistributionRule,
                    fx.ExpenseItemKey,
                    fx.ScopeType,
                    fx.ShopParticipationPercent,
                    fx.OwnerSplitPercent);

                if (!prev.Success)
                {
                    return new ExpenseDistributionPreviewResult
                    {
                        Success = false,
                        Message = $"'{fx.Title}' kaleminde dağıtım hesaplanamadı: {prev.Message}"
                    };
                }
                subPreviews.Add((fx.Title, prev));
            }

            if (activeStaff.Any())
            {
                decimal opTotal = activeStaff.Sum(s => s.TotalMonthlyOperatingCost);
                if (opTotal > 0m)
                {
                    var opPrev = await _governanceService.PreviewExpenseDistributionAsync(
                        buildingId,
                        opTotal,
                        BuildingExpenseType.Operational,
                        "Esit",
                        "Personel",
                        ExpenseScopeType.Building,
                        100m,
                        null);
                    if (!opPrev.Success) return opPrev;
                    subPreviews.Add(("Personel İşletme", opPrev));
                }

                decimal sevTotal = activeStaff.Sum(s => s.MonthlySeveranceFund);
                if (sevTotal > 0m)
                {
                    var sevPrev = await _governanceService.PreviewExpenseDistributionAsync(
                        buildingId,
                        sevTotal,
                        BuildingExpenseType.CapitalExpenditure,
                        "Esit",
                        "Personel",
                        ExpenseScopeType.Building,
                        100m,
                        100m);
                    if (!sevPrev.Success) return sevPrev;
                    subPreviews.Add(("Personel Kıdem (Malik)", sevPrev));
                }
            }

            var aggregated = new Dictionary<int, ExpenseDistributionPreviewItem>();
            var unitNotes = new Dictionary<int, List<string>>();

            foreach (var (title, prev) in subPreviews)
            {
                foreach (var item in prev.Items)
                {
                    if (!aggregated.TryGetValue(item.BuildingUnitId, out var acc))
                    {
                        acc = new ExpenseDistributionPreviewItem
                        {
                            BuildingUnitId = item.BuildingUnitId,
                            BuildingName = item.BuildingName,
                            DoorNumber = item.DoorNumber,
                            Category = item.Category,
                            RoomLayout = item.RoomLayout,
                            LandShare = item.LandShare,
                            GrossSquareMeters = item.GrossSquareMeters,
                            DuesMultiplier = item.DuesMultiplier,
                            ParticipationPercent = 100m,
                            EffectiveWeight = 1m,
                            OwnerName = item.OwnerName,
                            TenantName = item.TenantName
                        };
                        aggregated[item.BuildingUnitId] = acc;
                        unitNotes[item.BuildingUnitId] = new List<string>();
                    }

                    acc.OwnerAmount += item.OwnerAmount;
                    acc.TenantAmount += item.TenantAmount;
                    acc.TotalShareAmount += item.TotalShareAmount;

                    if (item.TotalShareAmount > 0m)
                    {
                        unitNotes[item.BuildingUnitId].Add($"{title}: {item.TotalShareAmount:N2}₺");
                    }
                    else if (!string.IsNullOrWhiteSpace(item.StatusNote))
                    {
                        unitNotes[item.BuildingUnitId].Add($"{title}: Muaf");
                    }
                }
            }

            var itemsList = aggregated.Values
                .OrderBy(x => x.BuildingName)
                .ThenBy(x => x.DoorNumber)
                .ToList();

            foreach (var x in itemsList)
            {
                var notes = unitNotes[x.BuildingUnitId];
                x.StatusNote = notes.Any()
                    ? string.Join(" | ", notes.Take(4)) + (notes.Count > 4 ? $" (+{notes.Count - 4} kalem)" : "")
                    : "Muaf";
            }

            return new ExpenseDistributionPreviewResult
            {
                Success = true,
                Message = $"{year}-{month:00} karma bütçe önizlemesi hazır.",
                ScopeLabel = $"Karma Bütçe ({subPreviews.Count} Kalem)",
                DistributionRule = "Karma",
                TotalAmount = itemsList.Sum(i => i.TotalShareAmount),
                ParticipatingUnitCount = itemsList.Count(i => i.TotalShareAmount > 0m),
                ExemptUnitCount = itemsList.Count(i => i.TotalShareAmount <= 0m),
                Items = itemsList
            };
        }

        public async Task<GovernanceActionResult> DistributeAllRecurringBudgetAsync(int buildingId, int year, int month, bool includeStaffCosts, string actorUserId)
        {
            var fixedItems = await _context.BuildingFixedExpenses
                .Where(f => f.BuildingId == buildingId && f.IsActive && f.IsAutoRecurring && !f.IsDeleted && f.MonthlyAmount > 0m)
                .OrderBy(f => f.DueDayOfMonth)
                .ToListAsync();

            var activeStaff = includeStaffCosts
                ? await _context.BuildingStaffs.Where(s => s.BuildingId == buildingId && s.IsActive && !s.IsDeleted).ToListAsync()
                : new List<BuildingStaff>();

            if (!fixedItems.Any() && !activeStaff.Any())
                return new GovernanceActionResult { Success = false, Message = "Bu bina için dağıtılacak otomatik sabit gider veya personel kaydı bulunamadı." };

            int postedCount = 0;
            decimal postedTotal = 0m;
            var warnings = new List<string>();

            foreach (var fx in fixedItems)
            {
                var res = await DistributeFixedExpenseAsync(fx.Id, year, month, actorUserId);
                if (res.Success)
                {
                    postedCount++;
                    postedTotal += fx.MonthlyAmount;
                }
                else
                {
                    warnings.Add($"{fx.Title}: {res.Message}");
                }
            }

            if (activeStaff.Any())
            {
                decimal opTotal = activeStaff.Sum(s => s.TotalMonthlyOperatingCost);
                if (opTotal > 0m)
                {
                    var opRes = await _governanceService.DistributeExpenseAsync(
                        buildingId,
                        $"Personel İşletme Bütçesi ({year}-{month:00} - {activeStaff.Count} Personel)",
                        opTotal,
                        BuildingExpenseType.Operational,
                        null,
                        actorUserId,
                        "Esit",
                        "Personel",
                        ExpenseScopeType.Building,
                        100m,
                        null,
                        "Personel");
                    if (opRes.Success)
                    {
                        postedCount++;
                        postedTotal += opTotal;
                    }
                    else
                    {
                        warnings.Add($"Personel İşletme: {opRes.Message}");
                    }
                }

                decimal sevTotal = activeStaff.Sum(s => s.MonthlySeveranceFund);
                if (sevTotal > 0m)
                {
                    var sevRes = await _governanceService.DistributeExpenseAsync(
                        buildingId,
                        $"Personel Kıdem Fonu ({year}-{month:00} - Malik Demirbaş)",
                        sevTotal,
                        BuildingExpenseType.CapitalExpenditure,
                        null,
                        actorUserId,
                        "Esit",
                        "Personel",
                        ExpenseScopeType.Building,
                        100m,
                        100m,
                        "Personel");
                    if (sevRes.Success)
                    {
                        postedCount++;
                        postedTotal += sevTotal;
                    }
                    else
                    {
                        warnings.Add($"Personel Kıdem: {sevRes.Message}");
                    }
                }
            }

            if (postedCount == 0)
            {
                return new GovernanceActionResult { Success = false, Message = string.Join(" | ", warnings) };
            }

            string warnSuffix = warnings.Any() ? $" (Uyarılar: {string.Join("; ", warnings)})" : "";
            return new GovernanceActionResult
            {
                Success = true,
                Message = $"✅ {year}-{month:00} dönemine ait {postedCount} kalem karma kurallarla paylaştırıldı (Toplam: {postedTotal:N2} ₺){warnSuffix}."
            };
        }
    }
}
