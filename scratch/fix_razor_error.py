import re

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Fix @keyframes to @@keyframes
content = content.replace('@keyframes pulse {', '@@keyframes pulse {')

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
