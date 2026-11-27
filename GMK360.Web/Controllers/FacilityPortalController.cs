using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using GMK360.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class FacilityPortalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FacilityTaskDispatchService _taskService;
        private readonly FacilityPayrollAndBudgetService _payrollService;

        public FacilityPortalController(
            ApplicationDbContext context,
            FacilityTaskDispatchService taskService,
            FacilityPayrollAndBudgetService payrollService)
        {
            _context = context;
            _taskService = taskService;
            _payrollService = payrollService;
        }

        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private bool IsManagerOrAdmin() =>
            User.IsInRole("Admin") || User.IsInRole("SuperAdmin") || User.IsInRole("BinaYoneticisi");

        public async Task<IActionResult> Index(string? previewRole = null, int? buildingId = null)
        {
            var userId = GetUserId();
            var userEmail = User.Identity?.Name;
            var user = !string.IsNullOrEmpty(userId)
                ? await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
                : null;

            bool isManager = IsManagerOrAdmin();

            // Yöneticinin yönettiği binalar
            var managedBuildings = isManager
                ? await _context.Buildings
                    .Where(b => b.ParentBuildingId == null
                        && (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")
                            || (b.BuildingManagers.Any(m => m.IsActive)
                                ? b.BuildingManagers.Any(m => m.UserId == userId && m.IsActive)
                                : (b.ManagerUserId == userId || b.CreatedByUserId == userId))))
                    .OrderByDescending(b => b.Id)
                    .ToListAsync()
                : new List<Building>();

            // Görevli eşleşmesi
            var normPhone = FacilityTaskDispatchService.NormalizePhoneLast10(user?.PhoneNumber);
            var staffProfile = await _context.BuildingStaffs
                .Include(s => s.HousingComplex)
                .Include(s => s.Building).ThenInclude(b => b.HousingComplex)
                .Include(s => s.Leaves)
                .Include(s => s.MonthlyPayrolls)
                .FirstOrDefaultAsync(s => s.IsActive && !s.IsDeleted
                    && ((userId != null && s.UserId == userId)
                        || (normPhone != "" && s.PhoneNumber != null && s.PhoneNumber.Contains(normPhone))));

            // Sakin / Malik eşleşmesi
            var residentUnits = await _context.BuildingUnits
                .Include(u => u.Building)
                .Include(u => u.Debts)
                    .ThenInclude(d => d.BuildingExpense)
                .Where(u => !(u.Category ?? "").StartsWith("OrtakAlan")
                    && ((userId != null && (u.OwnerUserId == userId || u.TenantUserId == userId))
                        || (!string.IsNullOrEmpty(userEmail) && (u.OwnerEmail == userEmail || u.TenantEmail == userEmail))))
                .ToListAsync();

            // Kurul / Aza / Denetçi eşleşmesi
            var boardMembership = userId != null
                ? await _context.BuildingManagers
                    .Include(m => m.Building)
                    .Where(m => m.UserId == userId && m.IsActive)
                    .OrderBy(m => m.Role)
                    .FirstOrDefaultAsync()
                : null;

            // Etkin görünüm rolü (Yönetici test etmek isterse previewRole ile 3 rolü de önizleyebilir)
            string activeRole = "Resident";
            if (isManager)
            {
                activeRole = previewRole switch
                {
                    "Staff" => "Staff",
                    "Owner" => "Owner",
                    "Tenant" => "Tenant",
                    _ => "Manager"
                };
            }
            else if (staffProfile != null || User.IsInRole("Personel"))
            {
                activeRole = "Staff";
            }
            else if (boardMembership != null && !residentUnits.Any())
            {
                activeRole = "Owner";
            }
            else
            {
                bool isTenantHere = residentUnits.Any(u => u.TenantUserId == userId || (!string.IsNullOrEmpty(userEmail) && u.TenantEmail == userEmail));
                activeRole = isTenantHere ? "Tenant" : "Owner";
            }

            int targetBuildingId = buildingId
                ?? managedBuildings.FirstOrDefault()?.Id
                ?? staffProfile?.BuildingId
                ?? residentUnits.FirstOrDefault()?.BuildingId
                ?? boardMembership?.BuildingId
                ?? 0;

            var portalBuildingsMap = new Dictionary<int, GMK360.Core.Entities.Building>();
            foreach (var b in managedBuildings) portalBuildingsMap[b.Id] = b;
            foreach (var u in residentUnits.Where(x => x.Building != null)) portalBuildingsMap[u.BuildingId] = u.Building;
            if (staffProfile?.Building != null) portalBuildingsMap[staffProfile.BuildingId] = staffProfile.Building;
            if (boardMembership?.Building != null) portalBuildingsMap[boardMembership.BuildingId] = boardMembership.Building;
            var portalBuildings = portalBuildingsMap.Values.OrderBy(b => b.Name).ThenBy(b => b.Id).ToList();

            if (targetBuildingId > 0 && !portalBuildingsMap.ContainsKey(targetBuildingId) && !isManager)
            {
                targetBuildingId = portalBuildings.FirstOrDefault()?.Id ?? 0;
            }

            var staffList = targetBuildingId > 0
                ? await _context.BuildingStaffs
                    .Include(s => s.Leaves)
                    .Include(s => s.MonthlyPayrolls)
                    .Where(s => s.BuildingId == targetBuildingId && s.IsActive && !s.IsDeleted)
                    .ToListAsync()
                : new List<BuildingStaff>();

            if (activeRole == "Staff" && staffProfile == null && isManager)
            {
                staffProfile = staffList.FirstOrDefault();
            }

            var scopedResidentUnits = targetBuildingId > 0
                ? residentUnits.Where(u => u.BuildingId == targetBuildingId).ToList()
                : residentUnits;

            var buildingUnitsForPreview = targetBuildingId > 0 && isManager && !scopedResidentUnits.Any()
                ? await _context.BuildingUnits
                    .Include(u => u.Building)
                    .Include(u => u.Debts).ThenInclude(d => d.BuildingExpense)
                    .Where(u => u.BuildingId == targetBuildingId && !(u.Category ?? "").StartsWith("OrtakAlan"))
                    .Take(5)
                    .ToListAsync()
                : scopedResidentUnits;

            // Eğer birden fazla binada rolü varsa seçili binaya göre Owner/Tenant ayrımını netleştir
            if (!isManager && activeRole != "Staff" && scopedResidentUnits.Any())
            {
                bool isTenantHere = scopedResidentUnits.Any(u => u.TenantUserId == userId || (!string.IsNullOrEmpty(userEmail) && u.TenantEmail == userEmail));
                bool isOwnerHere = scopedResidentUnits.Any(u => u.OwnerUserId == userId || (!string.IsNullOrEmpty(userEmail) && u.OwnerEmail == userEmail));
                activeRole = isOwnerHere ? "Owner" : (isTenantHere ? "Tenant" : activeRole);
            }

            var ticketsQuery = _context.BuildingTaskTickets
                .Include(t => t.Building)
                .Include(t => t.BuildingUnit)
                .Include(t => t.AssignedStaff)
                .Where(t => !t.IsDeleted);

            if (targetBuildingId > 0)
            {
                ticketsQuery = ticketsQuery.Where(t => t.BuildingId == targetBuildingId);
            }

            var tickets = await ticketsQuery.OrderByDescending(t => t.Id).Take(60).ToListAsync();
            if (activeRole == "Staff" && staffProfile != null)
            {
                tickets = tickets.Where(t => t.AssignedStaffId == staffProfile.Id).ToList();
            }
            else if ((activeRole == "Owner" || activeRole == "Tenant") && !isManager)
            {
                var myUnitIds = buildingUnitsForPreview.Select(u => u.Id).ToList();
                tickets = tickets.Where(t => t.CreatedByUserId == userId || (t.BuildingUnitId.HasValue && myUnitIds.Contains(t.BuildingUnitId.Value))).ToList();
            }

            var budgetSummary = targetBuildingId > 0
                ? await _payrollService.CalculateMonthlyBudgetSummaryAsync(targetBuildingId)
                : new BuildingMonthlyBudgetSummary();

            var targetBuilding = targetBuildingId > 0
                ? await _context.Buildings.Include(b => b.HousingComplex).AsNoTracking().FirstOrDefaultAsync(b => b.Id == targetBuildingId)
                : null;
            int rootBuildingId = targetBuilding?.ParentBuildingId ?? targetBuildingId;
            int? targetComplexId = targetBuilding?.HousingComplexId;
            var siteBlockIds = targetComplexId.HasValue
                ? await _context.Buildings.AsNoTracking().Where(b => b.HousingComplexId == targetComplexId.Value && b.ParentBuildingId == null).Select(b => b.Id).ToListAsync()
                : new List<int>();
            var ownBuildingIds = new List<int>();
            if (targetBuildingId > 0) ownBuildingIds.Add(targetBuildingId);
            if (targetBuilding?.ParentBuildingId.HasValue == true && rootBuildingId > 0 && !ownBuildingIds.Contains(rootBuildingId))
            {
                ownBuildingIds.Add(rootBuildingId);
            }
            if (!ownBuildingIds.Any() && managedBuildings.Any())
            {
                ownBuildingIds.AddRange(managedBuildings.Select(b => b.Id));
            }
            var unitQueryIds = new List<int>(ownBuildingIds);
            foreach (var sbid in siteBlockIds)
            {
                if (!unitQueryIds.Contains(sbid)) unitQueryIds.Add(sbid);
            }

            var meetings = (ownBuildingIds.Any() || targetComplexId.HasValue)
                ? await _context.SystemMeetings
                    .Include(m => m.Decisions)
                    .Include(m => m.Votes)
                    .Include(m => m.Attachments)
                    .Where(m => m.ContextType == "Building" && !m.IsDeleted
                        && m.WorkflowStage != "ArchivedNegative"
                        && (ownBuildingIds.Contains(m.ContextId)
                            || (m.IsSiteWide && m.HousingComplexId.HasValue && m.HousingComplexId == targetComplexId)))
                    .OrderBy(m => m.WorkflowStage == "Discussion" ? 0 : 1)
                    .ThenBy(m => m.IsConcluded)
                    .ThenByDescending(m => m.MeetingDate)
                    .Take(25)
                    .ToListAsync()
                : new List<SystemMeeting>();

            var allEligibleUnitsForMeetings = unitQueryIds.Any()
                ? await _context.BuildingUnits
                    .AsNoTracking()
                    .Where(u => unitQueryIds.Contains(u.BuildingId) && !(u.Category ?? "").StartsWith("OrtakAlan"))
                    .ToListAsync()
                : new List<BuildingUnit>();
            ViewBag.AllEligibleBuildingUnits = allEligibleUnitsForMeetings;

            var pendingApprovals = (boardMembership != null || isManager) && rootBuildingId > 0
                ? await _context.ApprovalRequests
                    .Where(a => a.BuildingId == rootBuildingId && a.Status == "PENDING")
                    .OrderByDescending(a => a.Id)
                    .ToListAsync()
                : new List<ApprovalRequest>();

            var buildingBankAccounts = targetBuildingId > 0
                ? await _context.BankAccounts
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.IsActive
                        && ((a.OwnerType == OwnerType.Building && (a.OwnerId == targetBuildingId || a.OwnerId == rootBuildingId))
                            || (targetBuilding != null && targetBuilding.HousingComplexId.HasValue && a.OwnerType == OwnerType.HousingComplex && a.OwnerId == targetBuilding.HousingComplexId.Value)))
                    .OrderByDescending(a => a.Id)
                    .ToListAsync()
                : new List<BankAccount>();

            var fixedExpenses = targetBuildingId > 0
                ? await _context.BuildingFixedExpenses
                    .AsNoTracking()
                    .Where(e => !e.IsDeleted && e.IsActive && (e.BuildingId == targetBuildingId || e.BuildingId == rootBuildingId))
                    .OrderBy(e => e.DueDayOfMonth)
                    .ToListAsync()
                : new List<BuildingFixedExpense>();

            var plannedMaintenances = targetBuildingId > 0
                ? await _context.SpaceFixtures
                    .AsNoTracking()
                    .Include(f => f.UnitSpace).ThenInclude(s => s.BuildingUnit)
                    .Where(f => !f.IsDeleted && f.NextMaintenanceDate.HasValue
                        && (f.UnitSpace.BuildingUnit.BuildingId == targetBuildingId || f.UnitSpace.BuildingUnit.BuildingId == rootBuildingId))
                    .OrderBy(f => f.NextMaintenanceDate)
                    .Take(12)
                    .ToListAsync()
                : new List<SpaceFixture>();

            var recentBuildingExpenses = targetBuildingId > 0
                ? await _context.BuildingExpenses
                    .AsNoTracking()
                    .Where(e => !e.IsDeleted && !e.IsArchived && (e.BuildingId == targetBuildingId || e.BuildingId == rootBuildingId))
                    .OrderByDescending(e => e.Date)
                    .Take(8)
                    .ToListAsync()
                : new List<BuildingExpense>();

            var meetingProposals = targetBuildingId > 0
                ? await _context.SupportTickets
                    .AsNoTracking()
                    .Include(t => t.CreatorUser)
                    .Where(t => !t.IsDeleted && t.ContextType == "Building"
                        && (t.ContextId == targetBuildingId || t.ContextId == rootBuildingId)
                        && t.Category == "MeetingRequest")
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(10)
                    .ToListAsync()
                : new List<SupportTicket>();

            var unreadNotifications = await _context.UserNotifications
                .AsNoTracking()
                .Where(n => n.ApplicationUserId == user.Id && !n.IsRead && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.ActiveRole = activeRole;
            ViewBag.CanSwitchPreview = isManager;
            ViewBag.UnreadNotifications = unreadNotifications;
            ViewBag.ManagedBuildings = managedBuildings;
            ViewBag.PortalBuildings = portalBuildings;
            ViewBag.SelectedBuilding = targetBuilding;
            ViewBag.SelectedBuildingId = targetBuildingId;
            ViewBag.RootBuildingId = rootBuildingId;
            ViewBag.StaffProfile = staffProfile;
            ViewBag.BuildingStaffList = staffList;
            ViewBag.ResidentUnits = buildingUnitsForPreview;
            ViewBag.Tickets = tickets;
            ViewBag.BudgetSummary = budgetSummary;
            ViewBag.Meetings = meetings;
            ViewBag.BoardMembership = boardMembership;
            ViewBag.PendingApprovals = pendingApprovals;
            ViewBag.CurrentUserProfile = user;
            ViewBag.BuildingBankAccounts = buildingBankAccounts;
            ViewBag.FixedExpenses = fixedExpenses;
            ViewBag.PlannedMaintenances = plannedMaintenances;
            ViewBag.RecentBuildingExpenses = recentBuildingExpenses;
            ViewBag.MeetingProposals = meetingProposals;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationsRead(int? buildingId, string? previewRole = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var unread = await _context.UserNotifications
                    .Where(n => n.ApplicationUserId == userId && !n.IsRead && !n.IsDeleted)
                    .ToListAsync();
                foreach (var n in unread)
                {
                    n.IsRead = true;
                    n.UpdatedAt = DateTime.UtcNow;
                }
                if (unread.Any())
                {
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index), new { buildingId, previewRole });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTaskTicket(
            int buildingId,
            int? buildingUnitId,
            string createdByRole,
            string category,
            string? locationNote,
            string description,
            IFormFile? beforePhoto,
            bool autoRouteCleaning = true,
            string priority = "Normal",
            string? previewRole = null)
        {
            if (buildingId <= 0 || string.IsNullOrWhiteSpace(description))
            {
                TempData["ErrorMessage"] = "Bina ve sorun açıklaması zorunludur.";
                return RedirectToAction(nameof(Index), new { previewRole, buildingId });
            }

            var ticket = await _taskService.CreateTicketAsync(
                buildingId,
                buildingUnitId,
                GetUserId(),
                createdByRole,
                category,
                locationNote,
                description,
                beforePhoto,
                autoRouteCleaning,
                priority);

            TempData["SuccessMessage"] = ticket.Status == "Atandi"
                ? $"Talebiniz #{ticket.Id} oluşturuldu ve otomatik olarak ilgili temizlik/kapıcı görevlisine atandı."
                : $"Fotoğraflı bildiriminiz (#{ticket.Id}) yöneticinin havuzuna iletildi.";

            return RedirectToAction(nameof(Index), new { previewRole, buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTaskTicket(int ticketId, int assignedStaffId, string priority, int buildingId, string? previewRole = null)
        {
            if (!IsManagerOrAdmin()) return Forbid();

            bool ok = await _taskService.AssignTicketAsync(ticketId, assignedStaffId, priority);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? $"Görev #{ticketId} personele atandı."
                : "Görev kaydı bulunamadı.";

            return RedirectToAction(nameof(Index), new { previewRole, buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTaskTicket(int ticketId, string? completionNote, IFormFile? afterPhoto, int buildingId, string? previewRole = null)
        {
            bool ok = await _taskService.CompleteTicketAsync(ticketId, completionNote, afterPhoto);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? $"Görev #{ticketId} kanıt fotoğrafıyla 'Tamamlandı' olarak kapatıldı."
                : "Görev bulunamadı.";

            return RedirectToAction(nameof(Index), new { previewRole, buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestStaffLeave(
            int buildingStaffId,
            int buildingId,
            string leaveType,
            DateTime startDate,
            DateTime endDate,
            string? notes,
            string? previewRole = null)
        {
            var res = await _payrollService.PlanStaffLeaveAsync(buildingStaffId, leaveType, startDate, endDate, 0m, notes);
            if (res.OverlapWarning != null) TempData["WarningMessage"] = res.Message;
            else TempData[res.Success ? "SuccessMessage" : "ErrorMessage"] = res.Message;

            return RedirectToAction(nameof(Index), new { previewRole, buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProposeMeetingAgenda(int buildingId, string agendaSubject, string? details, int? meetingId = null, string? previewRole = null)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Kiracı KMK toplantı gündemi açamaz; yalnızca Kat Maliki / Kurul Üyesi / Yönetici açabilir
            if (!IsManagerOrAdmin())
            {
                bool isOwnerOrBoard = await _context.BuildingManagers.AnyAsync(m => m.UserId == userId && m.IsActive)
                    || await _context.BuildingUnits.AnyAsync(u => u.BuildingId == buildingId && u.OwnerUserId == userId);
                if (!isOwnerOrBoard)
                {
                    TempData["ErrorMessage"] = "Toplantı gündem konusu açma yetkisi yalnızca Kat Malikleri ve Yönetim Kuruluna aittir.";
                    return RedirectToAction(nameof(Index), new { previewRole, buildingId });
                }
            }

            if (buildingId <= 0 || string.IsNullOrWhiteSpace(agendaSubject))
            {
                TempData["ErrorMessage"] = "Lütfen gündem başlığını yazınız.";
                return RedirectToAction(nameof(Index), new { previewRole, buildingId });
            }

            var building = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == buildingId);
            string assigneeId = !string.IsNullOrWhiteSpace(building?.ManagerUserId) ? building.ManagerUserId : userId;
            string prefix = meetingId.HasValue && meetingId.Value > 0 ? $"[Toplantı #{meetingId.Value} Gündem Maddesi]" : "[Kat Maliki Toplantı Konusu]";

            _context.SupportTickets.Add(new SupportTicket
            {
                Title = $"{prefix} {agendaSubject.Trim()}",
                Description = string.IsNullOrWhiteSpace(details) ? agendaSubject.Trim() : details.Trim(),
                Category = "MeetingRequest",
                Status = "Open",
                Priority = "Normal",
                CreatorUserId = userId,
                AssignedToUserId = assigneeId,
                ContextType = "Building",
                ContextId = buildingId,
                ContextInfo = meetingId.HasValue ? $"Toplantı #{meetingId.Value}" : "Kat Maliki Gündem Önerisi",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Gündem konunuz yönetime ve toplantı listesine eklendi.";
            return RedirectToAction(nameof(Index), new { previewRole, buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitBankPaymentNotice(
            int buildingId,
            int? buildingUnitId,
            decimal amount,
            int? bankAccountId,
            string? senderInfo,
            string? previewRole = null)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (buildingId <= 0 || amount <= 0)
            {
                TempData["ErrorMessage"] = "Geçerli bir tutar ve bina bilgisi seçiniz.";
                return RedirectToAction(nameof(Index), new { previewRole, buildingId });
            }

            var building = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == buildingId);
            var bank = bankAccountId.HasValue ? await _context.BankAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == bankAccountId.Value) : null;
            var unit = buildingUnitId.HasValue ? await _context.BuildingUnits.AsNoTracking().FirstOrDefaultAsync(u => u.Id == buildingUnitId.Value) : null;
            string assigneeId = !string.IsNullOrWhiteSpace(building?.ManagerUserId) ? building.ManagerUserId : userId;

            string bankSummary = bank != null ? $"{bank.BankName} ({bank.IBAN})" : "Bina Banka Hesabı";
            string unitLabel = unit != null ? $"Daire {unit.DoorNumber}" : "Daire";

            _context.SupportTickets.Add(new SupportTicket
            {
                Title = $"[IBAN Ödeme Bildirimi] {unitLabel} — {amount:N2} ₺",
                Description = $"Gönderen / Açıklama: {(string.IsNullOrWhiteSpace(senderInfo) ? User.Identity?.Name : senderInfo.Trim())}\nAlıcı Hesap: {bankSummary}\nTutar: {amount:N2} ₺\nNot: Yönetici tahsilat onayı sonrası borçtan düşülür.",
                Category = "PaymentNotice",
                Status = "Open",
                Priority = "Normal",
                CreatorUserId = userId,
                AssignedToUserId = assigneeId,
                ContextType = "Building",
                ContextId = buildingId,
                ContextInfo = unitLabel,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"💳 {amount:N2} ₺ tutarındaki IBAN havale/EFT bildiriminiz site yönetimine iletildi. Yönetici onayı sonrası bakiyenize işlenecektir.";
            return RedirectToAction(nameof(Index), new { previewRole, buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InitiateManagerTransfer(
            int buildingId,
            string newManagerEmail,
            string? newManagerFullName,
            string? newManagerPhone,
            string? newManagerTcNo,
            string? reason,
            [FromServices] FacilityGovernanceService governanceService,
            string? previewRole = null)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var building = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == buildingId);
            int rootId = building?.ParentBuildingId ?? buildingId;
            bool isBoardMember = IsManagerOrAdmin() || await _context.BuildingManagers.AnyAsync(m => m.BuildingId == rootId && m.UserId == userId && m.IsActive);
            if (!isBoardMember) return Forbid();

            var res = await governanceService.InitiateManagerTransferRequestAsync(
                buildingId, userId, newManagerEmail, newManagerFullName, newManagerPhone, newManagerTcNo, reason);
            TempData[res.Success ? "SuccessMessage" : "ErrorMessage"] = res.Message;

            return RedirectToAction(nameof(Index), new { previewRole, buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DecideBoardApproval(
            int requestId,
            int buildingId,
            bool approve,
            [FromServices] FacilityGovernanceService governanceService,
            string? confirmationPin = null,
            string? previewRole = null)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var building = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == buildingId);
            int rootId = building?.ParentBuildingId ?? buildingId;
            bool isBoardMember = IsManagerOrAdmin() || await _context.BuildingManagers.AnyAsync(m => m.BuildingId == rootId && m.UserId == userId && m.IsActive);
            if (!isBoardMember) return Forbid();

            var email = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "aza@gmk360.com";
            var result = await governanceService.SubmitApprovalDecisionAsync(requestId, userId, email, approve, IsManagerOrAdmin(), confirmationPin);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index), new { previewRole, buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRemoteDecisionVote(
            int buildingId,
            int meetingId,
            int buildingUnitId,
            string voteChoice,
            string? dissentNote = null,
            string? previewRole = null,
            [FromServices] FacilityGovernanceService? governanceService = null)
        {
            var userId = GetUserId();
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var meeting = await _context.SystemMeetings.FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building" && !m.IsDeleted);
            var unit = await _context.BuildingUnits.FirstOrDefaultAsync(u => u.Id == buildingUnitId && !u.IsEmpty);
            if (meeting == null || unit == null) return NotFound();
            if (!meeting.IsSiteWide && unit.BuildingId != meeting.ContextId && !IsManagerOrAdmin())
            {
                TempData["ErrorMessage"] = "634 sayılı KMK Madde 69 gereği yalnızca bu bloğun kat malikleri bu karar hakkında oy kullanabilir.";
                return RedirectToAction(nameof(Index), new { previewRole, buildingId });
            }
            if (meeting.IsConcluded)
            {
                TempData["ErrorMessage"] = "Bu toplantı karara bağlandığı için ön onay değiştirilemez.";
                return RedirectToAction(nameof(Index), new { previewRole, buildingId });
            }

            bool isOwner = unit.OwnerUserId == userId || (!string.IsNullOrWhiteSpace(userEmail) && string.Equals(unit.OwnerEmail, userEmail, StringComparison.OrdinalIgnoreCase));
            var existingVote = await _context.BuildingDecisionVotes
                .FirstOrDefaultAsync(v => v.SystemMeetingId == meetingId && v.BuildingUnitId == buildingUnitId && !v.IsDeleted);
            bool hasProxy = existingVote != null && existingVote.ParticipationType == "Vekil"
                && (unit.TenantUserId == userId || (!string.IsNullOrWhiteSpace(userEmail) && string.Equals(unit.TenantEmail, userEmail, StringComparison.OrdinalIgnoreCase)) || IsManagerOrAdmin());

            if (!isOwner && !hasProxy && !IsManagerOrAdmin())
            {
                TempData["ErrorMessage"] = "634 sayılı KMK gereği yalnızca Kat Maliki (veya yazılı vekalet tanımlanmış temsilcisi) ön onay verebilir.";
                return RedirectToAction(nameof(Index), new { previewRole, buildingId });
            }

            string cleanChoice = string.Equals(voteChoice, "Ret", StringComparison.OrdinalIgnoreCase) ? "Ret" : "Kabul";
            if (cleanChoice == "Ret" && string.IsNullOrWhiteSpace(dissentNote))
            {
                dissentNote = "Katılmıyorum (Şerh)";
            }

            string traceCode = $"ONAY-{Guid.NewGuid().ToString("N").Substring(0, 5).ToUpperInvariant()}";

            if (existingVote == null)
            {
                existingVote = new BuildingDecisionVote
                {
                    SystemMeetingId = meetingId,
                    BuildingUnitId = buildingUnitId,
                    VoterUserId = userId,
                    VoterName = !string.IsNullOrWhiteSpace(unit.OwnerName) ? unit.OwnerName : (User.Identity?.Name ?? "Kat Maliki"),
                    ParticipationType = isOwner ? "Asil" : "Vekil",
                    VoteChoice = cleanChoice,
                    DissentNote = string.IsNullOrWhiteSpace(dissentNote) ? null : dissentNote.Trim(),
                    UnitLandShareSnapshot = unit.LandShare,
                    ApprovalTraceCode = traceCode,
                    VotedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.BuildingDecisionVotes.Add(existingVote);
            }
            else
            {
                existingVote.VoterUserId = userId;
                existingVote.VoteChoice = cleanChoice;
                existingVote.DissentNote = cleanChoice == "Ret"
                    ? (string.IsNullOrWhiteSpace(dissentNote) ? "Katılmıyorum" : dissentNote.Trim())
                    : (string.IsNullOrWhiteSpace(dissentNote) ? null : dissentNote.Trim());
                existingVote.ApprovalTraceCode ??= traceCode;
                existingVote.VotedAt = DateTime.UtcNow;
                existingVote.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            if (cleanChoice == "Kabul" && governanceService != null)
            {
                var execRes = await governanceService.TryExecuteDecisionOnQuorumAsync(meetingId, userId, _taskService);
                if (execRes.Executed && !string.IsNullOrWhiteSpace(execRes.Message))
                {
                    TempData["SuccessMessage"] = $"✅ Daire {unit.DoorNumber} onayınız kaydedildi! Yasal çoğunluk sağlandığı için karar yürürlüğe girdi: {execRes.Message}";
                    return RedirectToAction(nameof(Index), new { previewRole, buildingId });
                }
            }

            TempData["SuccessMessage"] = cleanChoice == "Kabul"
                ? $"✅ Daire {unit.DoorNumber} için ön onayınız (Kabul) kaydedildi."
                : $"📝 Daire {unit.DoorNumber} için çekinceniz / şerhiniz kaydedildi.";
            return RedirectToAction(nameof(Index), new { previewRole, buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRemoteMeetingComment(
            int buildingId,
            int meetingId,
            string? commentText,
            string? documentTitle,
            IFormFile? attachmentFile,
            string? previewRole = null)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var meeting = await _context.SystemMeetings
                .Include(m => m.Votes)
                .FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building" && !m.IsDeleted);
            if (meeting == null) return NotFound();

            var posterEmail = User.FindFirstValue(ClaimTypes.Email);
            if (!meeting.IsSiteWide && !IsManagerOrAdmin())
            {
                bool belongsToBlock = await _context.BuildingUnits.AnyAsync(u =>
                    u.BuildingId == meeting.ContextId
                    && (u.OwnerUserId == userId || u.TenantUserId == userId
                        || (!string.IsNullOrEmpty(posterEmail) && (u.OwnerEmail == posterEmail || u.TenantEmail == posterEmail))));
                if (!belongsToBlock)
                {
                    TempData["ErrorMessage"] = "634 sayılı KMK Madde 69 gereği yalnızca ilgili bloğun kat malikleri bu görüşmeye katılım sağlayabilir.";
                    return RedirectToAction(nameof(Index), new { previewRole, buildingId });
                }
            }

            bool isTenantPoster = string.Equals(previewRole, "Tenant", StringComparison.OrdinalIgnoreCase);
            if (!IsManagerOrAdmin() && isTenantPoster)
            {
                bool isAllResidents = string.Equals(meeting.InvitationScope, "AllResidents", StringComparison.OrdinalIgnoreCase);
                bool hasTenantProxy = await _context.BuildingDecisionVotes.AnyAsync(v =>
                    v.SystemMeetingId == meeting.Id && !v.IsDeleted && v.ParticipationType == "Vekil"
                    && _context.BuildingUnits.Any(u => u.Id == v.BuildingUnitId && (u.TenantUserId == userId || (!string.IsNullOrEmpty(posterEmail) && u.TenantEmail == posterEmail))));
                if (!isAllResidents && !hasTenantProxy)
                {
                    TempData["ErrorMessage"] = "Bu toplantıda görüş/belge eklemek için malikinizin sizin adınıza vekalet tanımlamış olması gerekir.";
                    return RedirectToAction(nameof(Index), new { previewRole, buildingId });
                }
            }

            string? savedUrl = null;
            if (attachmentFile != null && attachmentFile.Length > 0 && attachmentFile.Length <= 15 * 1024 * 1024)
            {
                var ext = Path.GetExtension(attachmentFile.FileName)?.ToLowerInvariant() ?? "";
                var allowed = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp", ".doc", ".docx", ".xls", ".xlsx" };
                if (allowed.Contains(ext))
                {
                    var env = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)) as Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
                    var webRoot = env?.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var folder = Path.Combine(webRoot, "uploads", "buildings", buildingId.ToString(), "meeting-docs");
                    Directory.CreateDirectory(folder);
                    var uniqueName = $"r{meetingId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}".Substring(0, 32) + ext;
                    using (var stream = new FileStream(Path.Combine(folder, uniqueName), FileMode.Create))
                    {
                        await attachmentFile.CopyToAsync(stream);
                    }
                    savedUrl = $"/uploads/buildings/{buildingId}/meeting-docs/{uniqueName}";
                }
            }

            if (string.IsNullOrWhiteSpace(commentText) && string.IsNullOrWhiteSpace(savedUrl))
            {
                TempData["ErrorMessage"] = "Lütfen görüş notu yazın veya belge seçin.";
                return RedirectToAction(nameof(Index), new { previewRole, buildingId });
            }

            var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            string displayName = !string.IsNullOrWhiteSpace(currentUser?.FullName)
                ? currentUser.FullName
                : (User.Identity?.Name ?? "Kat Maliki");
            string roleLabel = string.Equals(previewRole, "Tenant", StringComparison.OrdinalIgnoreCase) ? "Kiracı"
                : IsManagerOrAdmin() ? "Yönetici" : "Kat Maliki";

            _context.BuildingDecisionAttachments.Add(new BuildingDecisionAttachment
            {
                SystemMeetingId = meeting.Id,
                AuthorUserId = userId,
                AuthorName = displayName,
                AuthorRoleLabel = roleLabel,
                CommentText = string.IsNullOrWhiteSpace(commentText) ? null : commentText.Trim(),
                DocumentTitle = !string.IsNullOrWhiteSpace(documentTitle) ? documentTitle.Trim() : attachmentFile?.FileName,
                FileUrl = savedUrl,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Görüşünüz / belgeniz toplantı dosyasına eklendi.";
            return RedirectToAction(nameof(Index), new { previewRole, buildingId });
        }
    }
}
