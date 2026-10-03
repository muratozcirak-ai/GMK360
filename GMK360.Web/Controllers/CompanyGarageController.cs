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
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class CompanyGarageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public CompanyGarageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        
        [HttpGet]
        public async Task<IActionResult> AllTasks(string? date)
        {
            ViewBag.Projects = await _context.ConstructionProjects.Where(p => !p.IsDeleted).Select(p => new { p.Id, p.Name }).ToListAsync();
            var agencyId = await GetUserAgencyIdAsync();
            var query = _context.VehicleTasks
                .Include(t => t.Vehicle)
                .Where(t => t.AgencyId == agencyId && !t.IsDeleted);

            if (!string.IsNullOrEmpty(date))
            {
                var parsedDate = DateTime.Parse(date).Date;
                query = query.Where(t => t.TaskDate.Date == parsedDate);
                ViewBag.SelectedDate = parsedDate.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewBag.SelectedDate = DateTime.Today.ToString("yyyy-MM-dd");
            }

            var tasks = await query.OrderByDescending(t => t.TaskDate).ToListAsync();
            return View(tasks);
        }

        [HttpGet("CompanyGarage/AllExpenses")]
        public async Task<IActionResult> AllExpenses(int? vehicleId, string expenseType, DateTime? startDate, DateTime? endDate)
        {
            var agencyId = 1; // Sabit
            var query = _context.VehicleExpenses.Include(e => e.Vehicle).Where(e => e.Vehicle.AgencyId == agencyId);
            
            if (vehicleId.HasValue) query = query.Where(e => e.VehicleId == vehicleId);
            if (!string.IsNullOrEmpty(expenseType)) query = query.Where(e => e.ExpenseType == expenseType);
            if (startDate.HasValue) query = query.Where(e => e.ExpenseDate >= startDate);
            if (endDate.HasValue) query = query.Where(e => e.ExpenseDate <= endDate);

            ViewBag.Vehicles = await _context.CompanyVehicles.Where(v => v.AgencyId == agencyId).ToListAsync();
            ViewBag.SelectedVehicleId = vehicleId;
            ViewBag.SelectedType = expenseType;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();
            return View(expenses);
        }

        [HttpPost("CompanyGarage/StartTask")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> StartTask(int taskId, int vehicleId, string driverName, int startKm)
        {
            var task = await _context.VehicleTasks.FindAsync(taskId);
            if (task != null)
            {
                task.VehicleId = vehicleId;
                task.DriverName = driverName;
                task.StartKm = startKm;
                task.Status = "Yolda / Görevde";
                
                var vehicle = await _context.CompanyVehicles.FindAsync(vehicleId);
                if(vehicle != null && startKm > vehicle.CurrentKm) {
                    vehicle.CurrentKm = startKm;
                }
                
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("AllTasks");
        }

        [HttpGet]

        public async Task<IActionResult> VehicleExpenses(int id)
        {
            var vehicle = await _context.CompanyVehicles
                .Include(v => v.Expenses)
                .FirstOrDefaultAsync(v => v.Id == id);
                
            if (vehicle == null) return NotFound();
            
            return View(vehicle);
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
            ViewBag.Projects = await _context.ConstructionProjects.Where(p => !p.IsDeleted).Select(p => new { p.Id, p.Name }).ToListAsync();
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
        public async Task<IActionResult> CreateTask(int vehicleId, string driverName, int? projectId, string description, string taskDate, string startTime, string endTime)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var vehicle = await _context.CompanyVehicles.FindAsync(vehicleId);

            var task = new VehicleTask
            {
                AgencyId = agencyId.Value,
                VehicleId = vehicleId,
                DriverName = driverName,
                DestinationProjectId = projectId,
                TaskDescription = description,
                TaskDate = string.IsNullOrEmpty(taskDate) ? DateTime.UtcNow : DateTime.Parse(taskDate),
                Status = "Bekliyor",
                StartKm = vehicle?.CurrentKm,
                StartTime = startTime,
                EndTime = endTime
            };

            _context.VehicleTasks.Add(task);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Araç Görevi başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, string status, int? endKm, decimal? workingHours)
        {
            var task = await _context.VehicleTasks.Include(t => t.Vehicle).FirstOrDefaultAsync(t => t.Id == taskId);
            if(task != null) {
                task.Status = status;
                
                if (endKm.HasValue && endKm.Value > (task.StartKm ?? 0))
                {
                    task.EndKm = endKm;
                    if (task.Vehicle != null) {
                        task.Vehicle.CurrentKm = endKm.Value;
                    }
                }
                
                if (workingHours.HasValue && workingHours.Value > 0)
                {
                    task.WorkingHours = workingHours;
                    if (task.Vehicle != null) {
                        task.Vehicle.CurrentWorkingHours += workingHours.Value;
                    }
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddExpense(int vehicleId, string expenseType, decimal amount, string receiptNumber, string expenseDate, int? odometer, Microsoft.AspNetCore.Http.IFormFile? photo)
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
                ExpenseDate = string.IsNullOrEmpty(expenseDate) ? DateTime.UtcNow : DateTime.Parse(expenseDate),
                OdometerAtExpense = odometer
            };
            
            if (photo != null && photo.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(photo.FileName);
                var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "garage");
                System.IO.Directory.CreateDirectory(uploadsFolder);
                var filePath = System.IO.Path.Combine(uploadsFolder, fileName);
                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }
                exp.PhotoPath = "/uploads/garage/" + fileName;
            }
            
            // Eğer girilen KM, aracın mevcut KM'sinden büyükse aracın güncel KM'sini de otomatik güncelle
            if (odometer.HasValue)
            {
                var vehicle = await _context.CompanyVehicles.FindAsync(vehicleId);
                if (vehicle != null && odometer.Value > vehicle.CurrentKm)
                {
                    vehicle.CurrentKm = odometer.Value;
                }
            }

            _context.VehicleExpenses.Add(exp);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Masraf/Fiş başarıyla araca işlendi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
