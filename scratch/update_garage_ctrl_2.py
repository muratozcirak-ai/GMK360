import codecs
import re

path = 'GMK360.Web/Controllers/CompanyGarageController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Make sure Microsoft.AspNetCore.Http, System.IO are included
if 'using Microsoft.AspNetCore.Http;' not in content:
    content = content.replace('using System.Threading.Tasks;', 'using System.Threading.Tasks;\nusing Microsoft.AspNetCore.Http;\nusing System.IO;\nusing Microsoft.AspNetCore.Hosting;')

# Add IWebHostEnvironment
if 'IWebHostEnvironment' not in content:
    content = content.replace('private readonly UserManager<ApplicationUser> _userManager;', 'private readonly UserManager<ApplicationUser> _userManager;\n        private readonly IWebHostEnvironment _env;')
    content = content.replace('public CompanyGarageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)', 'public CompanyGarageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)')
    content = content.replace('_userManager = userManager;', '_userManager = userManager;\n            _env = env;')

# Update AddExpense
target_addexp = '''public async Task<IActionResult> AddExpense(int vehicleId, string expenseType, decimal amount, string receiptNumber, string expenseDate, int? odometer)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var expense = new VehicleExpense
            {
                AgencyId = agencyId.Value,
                VehicleId = vehicleId,
                ExpenseType = expenseType,
                Amount = amount,
                ReceiptNumber = receiptNumber,
                ExpenseDate = string.IsNullOrEmpty(expenseDate) ? DateTime.UtcNow : DateTime.Parse(expenseDate),
                OdometerAtExpense = odometer
            };'''
replacement_addexp = '''public async Task<IActionResult> AddExpense(int vehicleId, string expenseType, decimal amount, string receiptNumber, string expenseDate, int? odometer, IFormFile? photo)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            string? photoPath = null;
            if (photo != null && photo.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "garage");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }
                photoPath = "/uploads/garage/" + uniqueFileName;
            }

            var expense = new VehicleExpense
            {
                AgencyId = agencyId.Value,
                VehicleId = vehicleId,
                ExpenseType = expenseType,
                Amount = amount,
                ReceiptNumber = receiptNumber,
                ExpenseDate = string.IsNullOrEmpty(expenseDate) ? DateTime.UtcNow : DateTime.Parse(expenseDate),
                OdometerAtExpense = odometer,
                PhotoPath = photoPath
            };'''
content = content.replace(target_addexp, replacement_addexp)

# Update CreateTask
target_createtask = '''public async Task<IActionResult> CreateTask(int vehicleId, string driverName, int? projectId, string description, string taskDate)'''
replacement_createtask = '''public async Task<IActionResult> CreateTask(int vehicleId, string driverName, int? projectId, string description, string taskDate, string startTime, string endTime)'''
content = content.replace(target_createtask, replacement_createtask)

target_createtask2 = '''Status = "Bekliyor",
                StartKm = vehicle?.CurrentKm
            };'''
replacement_createtask2 = '''Status = "Bekliyor",
                StartKm = vehicle?.CurrentKm,
                StartTime = startTime,
                EndTime = endTime
            };'''
content = content.replace(target_createtask2, replacement_createtask2)

# Add AllTasks and VehicleExpenses Actions
new_actions = '''
        [HttpGet]
        public async Task<IActionResult> AllTasks(string? date)
        {
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

        [HttpGet]
        public async Task<IActionResult> VehicleExpenses(int id)
        {
            var vehicle = await _context.CompanyVehicles
                .Include(v => v.Expenses)
                .FirstOrDefaultAsync(v => v.Id == id);
                
            if (vehicle == null) return NotFound();
            
            return View(vehicle);
        }
'''
content = content.replace('private async Task<int?> GetUserAgencyIdAsync()', new_actions + '\n        private async Task<int?> GetUserAgencyIdAsync()')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Updated Garage Controller completely')