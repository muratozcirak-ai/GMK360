import io
import re

def wipe_lines(filepath, substrings):
    try:
        with io.open(filepath, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        new_lines = []
        for line in lines:
            if any(s in line for s in substrings):
                continue
            new_lines.append(line)
        with io.open(filepath, 'w', encoding='utf-8') as f:
            f.writelines(new_lines)
    except:
        pass

# CustomerPortalController.cs
wipe_lines(r'GMK360.Web\Controllers\CustomerPortalController.cs', ['Phases', 'TaskMessages'])

# DailyTimesheetsController.cs
wipe_lines(r'GMK360.Web\Controllers\DailyTimesheetsController.cs', ['ProjectPhase', 'Phases'])

# AgencyWorkers/Details.cshtml
wipe_lines(r'GMK360.Web\Views\AgencyWorkers\Details.cshtml', ['ProjectPhase'])

# DailyTimesheets/Index.cshtml
wipe_lines(r'GMK360.Web\Views\DailyTimesheets\Index.cshtml', ['ProjectPhase'])

# CustomerPortal/MyUnitProgress.cshtml
wipe_lines(r'GMK360.Web\Views\CustomerPortal\MyUnitProgress.cshtml', ['TaskMessage', 'completedPhases', 'totalPhases', 'allPhases'])

# Details.cshtml
# Let's just remove anything with PhaseTasks, ProjectPhase, TaskMessage, Status == 2, etc.
wipe_lines(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', ['ProjectPhase', 'PhaseTasks', 'TaskMessage', 'Model.Phases', 'phase in Model'])

print("Cleaned lines")
