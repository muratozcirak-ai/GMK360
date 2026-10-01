import io
import re

filepath = r'GMK360.Web\Controllers\CustomerPortalController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = re.sub(r'\.Include\(p => p\.Phases\)', '', content)
content = re.sub(r'\.ThenInclude\(p => p\.PhaseTasks\)', '', content)
content = re.sub(r'var messages = await _context\.TaskMessages.*?;', 'var messages = new List<object>();', content, flags=re.DOTALL)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

filepath = r'GMK360.Web\Controllers\DailyTimesheetsController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = re.sub(r'\.Include\(t => t\.ProjectPhase\)', '', content)
content = re.sub(r'\.Include\(c => c\.Phases\)', '', content)
content = re.sub(r'PhaseName = t\.ProjectPhase \!= null \? t\.ProjectPhase\.Name : "-"', 'PhaseName = "-"', content)
content = re.sub(r't\.ProjectPhaseId == phaseId', 'false', content)
content = re.sub(r'ProjectPhaseId = phaseId', '', content)
content = re.sub(r'ProjectPhaseId = timesheet\.ProjectPhaseId', '', content)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

filepath = r'GMK360.Web\Views\DailyTimesheets\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = re.sub(r't\.ProjectPhaseId', '0', content)
content = re.sub(r't\.ProjectPhase', 'null', content)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
