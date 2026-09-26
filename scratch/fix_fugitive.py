import sys

filepath = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = "if (childDoc != null && !renderedDocIds.Contains(childDoc.Id))"
replacement = "if (childDoc != null)"

content = content.replace(target, replacement)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
