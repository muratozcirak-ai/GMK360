import io
import re

filepath = r'GMK360.Web\Views\ConstructionProject\_ProjectCardPartial.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix 1: Change card onclick
content = content.replace("onclick=\"location.href='/ConstructionProject/Details/@Model.Id'\"",
                          "onclick=\"window.open('/ConstructionProject/Details/@Model.Id', '_blank')\"")

# Fix 2: Inject target and onclick to all a tags targeting /ConstructionProject/Details/
content = re.sub(
    r'<a href="/ConstructionProject/Details/@Model\.Id"\s+class="',
    r'<a href="/ConstructionProject/Details/@Model.Id" target="_blank" onclick="event.stopPropagation();" class="',
    content
)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated _ProjectCardPartial.cshtml with regex")
