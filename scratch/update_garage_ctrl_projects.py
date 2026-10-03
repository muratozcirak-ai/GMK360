import codecs
import re

path = 'GMK360.Web/Controllers/CompanyGarageController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'public async Task<IActionResult> Index\(\)\s*\{'
replacement = '''public async Task<IActionResult> Index()
        {
            ViewBag.Projects = await _context.ConstructionProjects.Where(p => !p.IsDeleted).Select(p => new { p.Id, p.Name }).ToListAsync();'''
content = re.sub(target, replacement, content)

target_task = r'public async Task<IActionResult> CreateTask\(int vehicleId, string driverName, string taskDescription, DateTime taskDate, string startTime, string endTime\)'
replacement_task = r'public async Task<IActionResult> CreateTask(int vehicleId, string driverName, string taskDescription, DateTime taskDate, string startTime, string endTime, int? destinationProjectId)'
content = content.replace(target_task, replacement_task)

target_task_body = r'TaskDate = taskDate,'
replacement_task_body = r'TaskDate = taskDate, DestinationProjectId = destinationProjectId,'
content = content.replace(target_task_body, replacement_task_body)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('CompanyGarageController updated with Projects.')