using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Interfaces;
using GMK360.Core.Entities;
using GMK360.Web.Models.Dashboard;
using GMK360.Data.Contexts;

namespace GMK360.Web.Controllers
{
    public class ResidentPortalController : Controller
    {
        private readonly ApplicationDbContext _context;
        // In a real scenario, use UserManager to check if user is ShadowAccount

        public ResidentPortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Dummy check for current user, in real scenario get from User.Identity
            string userEmail = User.Identity?.Name ?? "shadow@example.com"; 

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            bool isShadow = user?.IsShadowAccount ?? false;

            var units = await _context.BuildingUnits
                .Include(u => u.Building)
                .Where(u => u.TenantEmail == userEmail || u.OwnerEmail == userEmail)
                .ToListAsync();

            var unitIds = units.Select(u => u.Id).ToList();

            var unpaidDebts = await _context.UnitDebts
                .Include(d => d.BuildingExpense)
                .Where(d => unitIds.Contains(d.BuildingUnitId) && !d.IsPaid)
                .ToListAsync();

            var model = new ResidentDashboardViewModel
            {
                MyUnits = units,
                UnpaidDebts = unpaidDebts,
                // If the user is a shadow account (just invited by manager), show them Cross-Sell CTAs
                ShowRenovationCta = isShadow,
                ShowPropertyEvaluationCta = isShadow,
                ShowInsuranceCta = isShadow
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            string userEmail = User.Identity?.Name ?? "shadow@example.com"; 

            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                .FirstOrDefaultAsync(u => u.Id == id && (u.TenantEmail == userEmail || u.OwnerEmail == userEmail));

            if (unit == null)
            {
                return NotFound("Daire bulunamadı veya yetkiniz yok.");
            }

            return View(unit);
        }

        // Karar Defteri (Sakinler için Salt Okunur)
        public async Task<IActionResult> Meetings()
        {
            string userEmail = User.Identity?.Name ?? "shadow@example.com"; 

            var buildingIds = await _context.BuildingUnits
                .Where(u => u.TenantEmail == userEmail || u.OwnerEmail == userEmail)
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
            string userEmail = User.Identity?.Name ?? "shadow@example.com"; 
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null) return NotFound();

            var tickets = await _context.SupportTickets
                .Where(t => t.CreatorUserId == user.Id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tickets);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket(string title, string description, string category, int contextId)
        {
            string userEmail = User.Identity?.Name ?? "shadow@example.com"; 
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null) return Unauthorized();

            // Find building manager to assign
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == contextId);

            var ticket = new SupportTicket
            {
                Title = title,
                Description = description,
                Category = category,
                Status = "Open",
                Priority = "Normal",
                CreatorUserId = user.Id,
                AssignedToUserId = building?.ManagerUserId, // Assign to building manager
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
