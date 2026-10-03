import codecs
import re

path = 'GMK360.Web/Controllers/CompanyGarageController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

assignment_action = '''
        [HttpPost]
        public async Task<IActionResult> AssignVehicle(int vehicleId, int? projectId, string assignedToName)
        {
            var activeAssignment = await _context.VehicleAssignments
                .Where(a => a.VehicleId == vehicleId && a.ReturnDate == null)
                .FirstOrDefaultAsync();
                
            if (activeAssignment != null)
            {
                activeAssignment.ReturnDate = DateTime.UtcNow;
            }

            var newAssignment = new VehicleAssignment
            {
                AgencyId = 1,
                VehicleId = vehicleId,
                ProjectId = projectId,
                AssignedToName = assignedToName,
                AssignmentDate = DateTime.UtcNow
            };
            
            _context.VehicleAssignments.Add(newAssignment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
'''
if 'AssignVehicle(int' not in content:
    content = content.replace('public async Task<IActionResult> CreateTask', assignment_action + '\n        public async Task<IActionResult> CreateTask')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)