import sys

with open('GMK360.Web/Views/ConstructionProject/Amenities.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# Make the form pass isExisting
html = html.replace(
    '<input type="hidden" name="projectId" value="@Model.Id" />\n                        <div class="mb-4">',
    '<input type="hidden" name="projectId" value="@Model.Id" />\n                        <input type="hidden" name="isExisting" value="@(ViewBag.IsExisting == true ? "true" : "false")" />\n                        <div class="mb-4">'
)

# Update page title depending on IsExisting
html = html.replace(
    '<h2 class="fw-bold mb-1">Sosyal Donatılar ve Dış Alanlar</h2>',
    '<h2 class="fw-bold mb-1">Sosyal Donatılar ve Dış Alanlar @(ViewBag.IsExisting == true ? "(Eski / Yıkılacak Durum)" : "(Yeni / Hedef Durum)")</h2>'
)

# Update the calculations in Amenities.cshtml
calc_old = 'double totalBlocksFootprint = Model.Blocks?.Where(b => !b.IsExistingBuilding).Sum(b => b.BaseArea ?? 0) ?? 0;'
calc_new = '''bool isEx = ViewBag.IsExisting == true;
                      double totalBlocksFootprint = Model.Blocks?.Where(b => b.IsExistingBuilding == isEx).Sum(b => b.BaseArea ?? 0) ?? 0;'''
html = html.replace(calc_old, calc_new)

html = html.replace(
    'double totalAmenities = Model.Amenities?.Sum(a => a.SquareMeters ?? 0) ?? 0;',
    'double totalAmenities = Model.Amenities?.Where(a => a.IsExisting == isEx).Sum(a => a.SquareMeters ?? 0) ?? 0;'
)

html = html.replace(
    '@foreach(var block in Model.Blocks.Where(b => !b.IsExistingBuilding))',
    '@foreach(var block in Model.Blocks.Where(b => b.IsExistingBuilding == isEx))'
)
html = html.replace(
    '@if(Model.Blocks != null && Model.Blocks.Any(b => !b.IsExistingBuilding))',
    '@if(Model.Blocks != null && Model.Blocks.Any(b => b.IsExistingBuilding == isEx))'
)

# Filter the amenity list
html = html.replace(
    '@foreach (var item in Model.Amenities.OrderBy(a => a.Type).ThenBy(a => a.Name))',
    '@foreach (var item in Model.Amenities.Where(a => a.IsExisting == isEx).OrderBy(a => a.Type).ThenBy(a => a.Name))'
)
html = html.replace(
    '@if (Model.Amenities != null && Model.Amenities.Any())',
    '@if (Model.Amenities != null && Model.Amenities.Any(a => a.IsExisting == isEx))'
)


with open('GMK360.Web/Views/ConstructionProject/Amenities.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)

print("SUCCESS")
