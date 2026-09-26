import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Update GET Method
pattern_get = r'(model\.ExistingBlocks = .*?IsExistingBuilding = true,.*?LayoutPattern = b\.LayoutPattern)'
content = re.sub(pattern_get, r'\1, BuildingAge = b.BuildingAge', content, flags=re.DOTALL)

# Update POST Method mapping (Update existing block)
pattern_post1 = r'(blockEntity\.IsExistingBuilding = b\.IsExistingBuilding;\s*blockEntity\.LayoutPattern = b\.LayoutPattern;)'
content = re.sub(pattern_post1, r'\1\n                            blockEntity.BuildingAge = b.BuildingAge;', content, flags=re.DOTALL)

# Update POST Method mapping (New block)
pattern_post2 = r'(IsExistingBuilding = b\.IsExistingBuilding,\s*LayoutPattern = b\.LayoutPattern)'
content = re.sub(pattern_post2, r'\1,\n                                BuildingAge = b.BuildingAge', content, flags=re.DOTALL)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Controller mapping updated.")
