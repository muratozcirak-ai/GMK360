using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Interfaces;
using GMK360.Core.Entities;
using GMK360.Web.Models.Dashboard;
using GMK360.Data.Contexts;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class ResidentPortalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResidentPortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<(GMK360.Core.Entities.Identity.ApplicationUser? User, string? Email)> GetCurrentResidentAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.Identity?.Name;
            GMK360.Core.Entities.Identity.ApplicationUser? user = null;

            if (!string.IsNullOrEmpty(userId))
                user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null && !string.IsNullOrEmpty(email))
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            return (user, user?.Email ?? email);
        }

        private IQueryable<BuildingUnit> GetUserUnitsQuery(string? userId, string? userEmail)
        {
            return _context.BuildingUnits
                .Include(u => u.Building)
                .Where(u =>
                    (!string.IsNullOrEmpty(userEmail) && (u.TenantEmail == userEmail || u.OwnerEmail == userEmail)) ||
                    (!string.IsNullOrEmpty(userId) && (u.TenantUserId == userId || u.OwnerUserId == userId)));
        }

        public async Task<IActionResult> Index()
        {
            var (user, userEmail) = await GetCurrentResidentAsync();
            if (user == null && string.IsNullOrEmpty(userEmail)) return Challenge();

            bool isShadow = user?.IsShadowAccount ?? false;

            var units = await GetUserUnitsQuery(user?.Id, userEmail).ToListAsync();
            var unitIds = units.Select(u => u.Id).ToList();

            var unpaidDebts = await _context.UnitDebts
                .Include(d => d.BuildingExpense)
                .Where(d => unitIds.Contains(d.BuildingUnitId) && !d.IsPaid)
                .ToListAsync();

            var model = new ResidentDashboardViewModel
            {
                MyUnits = units,
                UnpaidDebts = unpaidDebts,
                ShowRenovationCta = isShadow,
                ShowPropertyEvaluationCta = isShadow,
                ShowInsuranceCta = isShadow
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var (user, userEmail) = await GetCurrentResidentAsync();
            if (user == null && string.IsNullOrEmpty(userEmail)) return Challenge();

            var unit = await GetUserUnitsQuery(user?.Id, userEmail)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null)
            {
                return NotFound("Daire bulunamadı veya yetkiniz yok.");
            }

            return View(unit);
        }

        // Karar Defteri (Sakinler için Salt Okunur)
        public async Task<IActionResult> Meetings()
        {
            var (user, userEmail) = await GetCurrentResidentAsync();
            if (user == null && string.IsNullOrEmpty(userEmail)) return Challenge();

            var buildingIds = await GetUserUnitsQuery(user?.Id, userEmail)
                .Select(u => u.BuildingId)
                .Distinct()
                .ToListAsync();

            var meetings = await _context.SystemMeetings
                .Include(m => m.Decisions)
                .Where(m => m.ContextType == "Building" && buildingIds.Contains(m.ContextId) && m.IsConcluded && !m.IsDeleted)
                .OrderByDescending(m => m.MeetingDate)
                .ToListAsync();

            return View(meetings);
        }

        // İç Talepler (Support Tickets)
        public async Task<IActionResult> Tickets()
        {
            var (user, userEmail) = await GetCurrentResidentAsync();
            if (user == null) return Challenge();

            var tickets = await _context.SupportTickets
                .Where(t => t.CreatorUserId == user.Id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.UserBuildings = await GetUserUnitsQuery(user.Id, userEmail)
                .Select(u => u.Building)
                .Where(b => b != null)
                .Distinct()
                .ToListAsync();

            return View(tickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTicket(string title, string description, string category, int contextId = 0)
        {
            var (user, userEmail) = await GetCurrentResidentAsync();
            if (user == null) return Unauthorized();

            var userBuildingIds = await GetUserUnitsQuery(user.Id, userEmail)
                .Select(u => u.BuildingId)
                .Distinct()
                .ToListAsync();

            if (contextId <= 0)
            {
                contextId = userBuildingIds.FirstOrDefault();
            }
            else if (!userBuildingIds.Contains(contextId) && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            var building = contextId > 0
                ? await _context.Buildings.FirstOrDefaultAsync(b => b.Id == contextId)
                : null;

            var ticket = new SupportTicket
            {
                Title = title,
                Description = description,
                Category = category,
                Status = "Open",
                Priority = "Normal",
                CreatorUserId = user.Id,
                AssignedToUserId = building?.ManagerUserId,
                ContextType = "Building",
                ContextId = contextId,
                ContextInfo = "Yeni Talep",
                CreatedAt = System.DateTime.Now
            };

            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Talebiniz başarıyla yönetime iletildi.";
            return RedirectToAction(nameof(Tickets));
        }
    }
}
