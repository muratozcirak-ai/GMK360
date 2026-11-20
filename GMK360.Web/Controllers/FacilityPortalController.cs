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
            else
            {
                bool isTenantHere = residentUnits.Any(u => u.TenantUserId == userId || (!string.IsNullOrEmpty(userEmail) && u.TenantEmail == userEmail));
                activeRole = isTenantHere ? "Tenant" : "Owner";
            }

            int targetBuildingId = buildingId
                ?? managedBuildings.FirstOrDefault()?.Id
                ?? staffProfile?.BuildingId
                ?? residentUnits.FirstOrDefault()?.BuildingId
                ?? 0;

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

            var buildingUnitsForPreview = targetBuildingId > 0 && isManager && !residentUnits.Any()
                ? await _context.BuildingUnits
                    .Include(u => u.Building)
                    .Include(u => u.Debts).ThenInclude(d => d.BuildingExpense)
                    .Where(u => u.BuildingId == targetBuildingId && !(u.Category ?? "").StartsWith("OrtakAlan"))
                    .Take(5)
                    .ToListAsync()
                : residentUnits;

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
                var myUnitIds = residentUnits.Select(u => u.Id).ToList();
                tickets = tickets.Where(t => t.CreatedByUserId == userId || (t.BuildingUnitId.HasValue && myUnitIds.Contains(t.BuildingUnitId.Value))).ToList();
            }

            var budgetSummary = targetBuildingId > 0
                ? await _payrollService.CalculateMonthlyBudgetSummaryAsync(targetBuildingId)
                : new BuildingMonthlyBudgetSummary();

            ViewBag.ActiveRole = activeRole;
            ViewBag.CanSwitchPreview = isManager;
            ViewBag.ManagedBuildings = managedBuildings;
            ViewBag.SelectedBuildingId = targetBuildingId;
            ViewBag.StaffProfile = staffProfile;
            ViewBag.BuildingStaffList = staffList;
            ViewBag.ResidentUnits = buildingUnitsForPreview;
            ViewBag.Tickets = tickets;
            ViewBag.BudgetSummary = budgetSummary;

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
    }
}
