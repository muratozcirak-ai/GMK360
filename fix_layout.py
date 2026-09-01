import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\CustomerDashboard\SetupCorporateProfile.cshtml"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("_CustomerLayout.cshtml", "_Layout.cshtml")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Fixed layout.")
