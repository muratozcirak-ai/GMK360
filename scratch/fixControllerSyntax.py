import re

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'r', encoding='utf-8') as f:
    code = f.read()

code = code.replace(
    'amenity.Description = description,\n                IsExisting = isExisting;',
    'amenity.Description = description;\n                // amenity.IsExisting = isExisting;'
)

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'w', encoding='utf-8') as f:
    f.write(code)

print("SUCCESS")
