import io

filepath = r'GMK360.Web\Controllers\DailyTimesheetController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("ViewBag.ProjectName = project.ProjectName;", "ViewBag.ProjectName = project.Name;")

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
