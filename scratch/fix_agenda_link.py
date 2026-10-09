import re

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Fix meeting link
content = content.replace('href="/Meetings/Index"', 'href="/Agenda/Index"')

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
