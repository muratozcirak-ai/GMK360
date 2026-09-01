import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('return RedirectToAction("Index", "Dashboard");', 'return RedirectToAction("SetupCorporateProfile", "CustomerDashboard");')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated ConstructionProjectController redirection.")
