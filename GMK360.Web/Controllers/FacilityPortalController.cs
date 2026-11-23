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
                .Include(s => s.Building)
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
                ? await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == targetBuildingId)
                : null;
            int rootBuildingId = targetBuilding?.ParentBuildingId ?? targetBuildingId;
            var meetingBuildingIds = new List<int>();
            if (targetBuildingId > 0) meetingBuildingIds.Add(targetBuildingId);
            if (rootBuildingId > 0 && rootBuildingId != targetBuildingId) meetingBuildingIds.Add(rootBuildingId);

            var meetings = meetingBuildingIds.Any()
                ? await _context.SystemMeetings
                    .Include(m => m.Decisions)
                    .Where(m => m.ContextType == "Building" && meetingBuildingIds.Contains(m.ContextId) && !m.IsDeleted)
                    .OrderBy(m => m.IsConcluded)
                    .ThenByDescending(m => m.MeetingDate)
                    .Take(20)
                    .ToListAsync()
                : new List<SystemMeeting>();

            var pendingApprovals = (boardMembership != null || isManager) && rootBuildingId > 0
                ? await _context.ApprovalRequests
                    .Where(a => a.BuildingId == rootBuildingId && a.Status == "PENDING")
                    .OrderByDescending(a => a.Id)
                    .ToListAsync()
                : new List<ApprovalRequest>();

            ViewBag.ActiveRole = activeRole;
            ViewBag.CanSwitchPreview = isManager;
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

            return View();
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
        public async Task<IActionResult> ProposeMeetingAgenda(int buildingId, string agendaSubject, string? details, string? previewRole = null)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (buildingId <= 0 || string.IsNullOrWhiteSpace(agendaSubject))
            {
                TempData["ErrorMessage"] = "Lütfen gündem başlığını yazınız.";
                return RedirectToAction(nameof(Index), new { previewRole, buildingId });
            }

            var building = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == buildingId);
            string assigneeId = !string.IsNullOrWhiteSpace(building?.ManagerUserId) ? building.ManagerUserId : userId;

            _context.SupportTickets.Add(new SupportTicket
            {
                Title = $"[Toplantı / Gündem Talebi] {agendaSubject.Trim()}",
                Description = string.IsNullOrWhiteSpace(details) ? agendaSubject.Trim() : details.Trim(),
                Category = "MeetingRequest",
                Status = "Open",
                Priority = "Normal",
                CreatorUserId = userId,
                AssignedToUserId = assigneeId,
                ContextType = "Building",
                ContextId = buildingId,
                ContextInfo = "Sakin / Aza Toplantı Önerisi",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Toplantı / gündem öneriniz site yönetimine iletildi.";
            return RedirectToAction(nameof(Index), new { previewRole, buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DecideBoardApproval(
            int requestId,
            int buildingId,
            bool approve,
            [FromServices] FacilityGovernanceService governanceService,
            string? previewRole = null)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var building = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == buildingId);
            int rootId = building?.ParentBuildingId ?? buildingId;
            bool isBoardMember = IsManagerOrAdmin() || await _context.BuildingManagers.AnyAsync(m => m.BuildingId == rootId && m.UserId == userId && m.IsActive);
            if (!isBoardMember) return Forbid();

            var email = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "aza@gmk360.com";
            var result = await governanceService.SubmitApprovalDecisionAsync(requestId, userId, email, approve, IsManagerOrAdmin());
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index), new { previewRole, buildingId });
        }
    }
}
