import re

filepath = r'GMK360.Web\Views\Shared\_ProjectLayout.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace ManagePhases with BudgetDashboard in the sidebar link
content = content.replace('href="@Url.Action("ManagePhases", "ConstructionProject", new { id = ViewData["ProjectId"] })"', 'href="@Url.Action("BudgetDashboard", "ConstructionProject", new { id = ViewData["ProjectId"] })"')
content = content.replace('ViewContext.RouteData.Values["Action"]?.ToString() == "ManagePhases"', 'ViewContext.RouteData.Values["Action"]?.ToString() == "BudgetDashboard"')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
