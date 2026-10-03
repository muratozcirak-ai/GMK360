import codecs
import re

path = 'GMK360.Web/Controllers/CompanyGarageController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Add AllExpenses action
all_expenses_code = '''
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
'''

if 'AllExpenses(int?' not in content:
    content = content.replace('public async Task<IActionResult> VehicleExpenses(int id)', all_expenses_code + '\n        public async Task<IActionResult> VehicleExpenses(int id)')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Controller updated.')