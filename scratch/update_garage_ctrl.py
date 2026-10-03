import codecs

path = 'GMK360.Web/Controllers/CompanyGarageController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

create_task_old = '''        public async Task<IActionResult> CreateTask(int vehicleId, string driverName, int? projectId, string description, string taskDate)
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

            _context.VehicleTasks.Add(task);'''

create_task_new = '''        public async Task<IActionResult> CreateTask(int vehicleId, string driverName, int? projectId, string description, string taskDate)
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
                StartKm = vehicle?.CurrentKm
            };

            _context.VehicleTasks.Add(task);'''

update_task_old = '''        public async Task<IActionResult> UpdateTaskStatus(int taskId, string status)
        {
            var task = await _context.VehicleTasks.FindAsync(taskId);
            if(task != null) {
                task.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }'''

update_task_new = '''        public async Task<IActionResult> UpdateTaskStatus(int taskId, string status, int? endKm, decimal? workingHours)
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
        }'''

content = content.replace(create_task_old, create_task_new)
content = content.replace(update_task_old, update_task_new)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Updated Garage Controller for KM')