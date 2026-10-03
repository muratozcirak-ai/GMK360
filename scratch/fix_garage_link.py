import codecs
import re

path = 'GMK360.Web/Views/Shared/_ConstructionLayout.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'<a href="/CompanyGarage" class="list-group-item list-group-item-action @\(ViewContext.RouteData.Values\["Controller"\]\?\.ToString\(\) == "CompanyGarage" \? "active" : ""\)">'
replacement = r'<a href="/CompanyGarage" target="_blank" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "CompanyGarage" ? "active" : "")">'
content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Added target=_blank to CompanyGarage link.')