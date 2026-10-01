using GMK360.Core.Entities.Logistics;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class CompanyGarageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyGarageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) return 1;
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive);
            return consultant?.AgencyId;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var vehicles = await _context.CompanyVehicles
                .Include(v => v.Assignments)
                .Include(v => v.Tasks)
                .Include(v => v.Expenses)
                .Where(v => v.AgencyId == agencyId.Value && v.Status == "Aktif")
                .ToListAsync();

            ViewBag.ActiveProjects = await _context.ConstructionProjects
                .Where(p => p.AgencyId == agencyId.Value)
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();

            return View(vehicles);
        }

        [HttpPost]
        public async Task<IActionResult> AddVehicle(string plateNumber, string type, string brand)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var vehicle = new CompanyVehicle
            {
                AgencyId = agencyId.Value,
                PlateNumber = plateNumber,
                VehicleType = type,
                BrandModel = brand,
                Status = "Aktif"
            };

            _context.CompanyVehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AssignVehicle(int vehicleId, string assignedTo, int? projectId, string notes)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var activeAssign = await _context.VehicleAssignments
                .FirstOrDefaultAsync(a => a.VehicleId == vehicleId && a.ReturnDate == null);
            
            if (activeAssign != null) activeAssign.ReturnDate = DateTime.UtcNow;

            var newAssign = new VehicleAssignment
            {
                AgencyId = agencyId.Value,
                VehicleId = vehicleId,
                AssignedToName = assignedTo,
                ProjectId = projectId,
                Notes = notes,
                AssignmentDate = DateTime.UtcNow
            };

            _context.VehicleAssignments.Add(newAssign);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Araç başarıyla zimmetlendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(int vehicleId, string driverName, int? projectId, string description, string taskDate)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var task = new VehicleTask
            {
                AgencyId = agencyId.Value,
                VehicleId = vehicleId,
                DriverName = driverName,
                DestinationProjectId = projectId,
                TaskDescription = description,
                TaskDate = string.IsNullOrEmpty(taskDate) ? DateTime.UtcNow : DateTime.Parse(taskDate),
                Status = "Bekliyor"
            };

            _context.VehicleTasks.Add(task);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Araç Görevi başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, string status)
        {
            var task = await _context.VehicleTasks.FindAsync(taskId);
            if(task != null) {
                task.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddExpense(int vehicleId, string expenseType, decimal amount, string receiptNumber, string expenseDate)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var exp = new VehicleExpense
            {
                AgencyId = agencyId.Value,
                VehicleId = vehicleId,
                ExpenseType = expenseType,
                Amount = amount,
                ReceiptNumber = receiptNumber,
                ReportedBy = _userManager.GetUserName(User),
                ExpenseDate = string.IsNullOrEmpty(expenseDate) ? DateTime.UtcNow : DateTime.Parse(expenseDate)
            };

            _context.VehicleExpenses.Add(exp);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Masraf/Fiş başarıyla araca işlendi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
