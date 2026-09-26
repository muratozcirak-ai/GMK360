import sys

filepath = 'GMK360.Web/Views/Admin/GlobalProviders.cshtml'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = "item.AddedByAgency.Name"
replacement = "item.AddedByAgency.CompanyName"

if target in content:
    content = content.replace(target, replacement)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        print("GlobalProviders view updated.")
else:
    print("GlobalProviders view already updated.")
