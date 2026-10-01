import io
import re

filepath = r'GMK360.Web\Controllers\DailyTimesheetController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# I need to insert a "    }" right before "    public class TimesheetSaveRequest"
content = content.replace("    public class TimesheetSaveRequest", "    }\n\n    public class TimesheetSaveRequest")

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Fixed brackets")
