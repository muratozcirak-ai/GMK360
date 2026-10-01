import io
import re

filepath = r'GMK360.Web\Controllers\BuildingManagementController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("agency?.Name", "agency?.CompanyName")

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Fixed CompanyName property")
