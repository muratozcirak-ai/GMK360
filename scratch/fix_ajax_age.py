import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

pattern = r'binaYasi = project\.BuildingAge \?\? 0'
# We will change it to fetch from blocks
# var blocks = await _context.Buildings.Where(b => b.ConstructionProjectId == id && b.IsExistingBuilding).ToListAsync();
# var maxAge = blocks.Max(b => (int?)b.BuildingAge) ?? 0;
replacement = r'binaYasi = _context.Buildings.Where(b => b.ConstructionProjectId == id && b.IsExistingBuilding).Max(b => (int?)b.BuildingAge) ?? 0'

content = re.sub(pattern, replacement, content)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("GetOldBuildingStats updated.")
