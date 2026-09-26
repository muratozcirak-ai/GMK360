import sys

filepath = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("ConstructionProjectId = id,", "ConstructionProjectId = id.Value,")

with open(filepath, 'w', encoding='utf-8-sig') as f:
    f.write(content)
