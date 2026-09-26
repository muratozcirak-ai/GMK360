import sys

with open('GMK360.Web/Views/ConstructionProject/Amenities.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# Fix the total calculation
html = html.replace(
    'double totalBlocksFootprint = Model.Blocks?.Sum(b => b.BaseArea ?? 0) ?? 0;',
    'double totalBlocksFootprint = Model.Blocks?.Where(b => !b.IsExistingBuilding).Sum(b => b.BaseArea ?? 0) ?? 0;'
)

# Fix the loop showing the blocks
html = html.replace(
    '@foreach(var block in Model.Blocks)',
    '@foreach(var block in Model.Blocks.Where(b => !b.IsExistingBuilding))'
)

# Wait, if there are NO new blocks?
html = html.replace(
    '@if(Model.Blocks != null && Model.Blocks.Any())',
    '@if(Model.Blocks != null && Model.Blocks.Any(b => !b.IsExistingBuilding))'
)

with open('GMK360.Web/Views/ConstructionProject/Amenities.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)

print("SUCCESS")
