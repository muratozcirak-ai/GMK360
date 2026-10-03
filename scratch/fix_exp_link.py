import codecs
import re

path = 'GMK360.Web/Views/Shared/_ConstructionLayout.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'<a href="/ConstructionProjectExpenses" class="list-group-item list-group-item-action @\(ViewContext.RouteData.Values\["Controller"\]\?\.ToString\(\) == "ConstructionProjectExpenses" \? "active" : ""\)">'
replacement = r'<a href="/ConstructionProjectExpenses" target="_blank" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "ConstructionProjectExpenses" ? "active" : "")">'
content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Added target=_blank to ConstructionProjectExpenses link.')