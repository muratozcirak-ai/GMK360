import re

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'r', encoding='utf-8') as f:
    code = f.read()

# Modify Amenities GET
code = code.replace(
    'public async Task<IActionResult> Amenities(int projectId)',
    'public async Task<IActionResult> Amenities(int projectId, bool isExisting = false)'
)

# Add ViewBag to Amenities GET
insertion = '''
            ViewBag.IsExisting = isExisting;
            if (project == null'''
code = code.replace('if (project == null', insertion, 1)

# Modify AddAmenity POST
code = code.replace(
    'public async Task<IActionResult> AddAmenity(int projectId, string name, string type, double? squareMeters, string description)',
    'public async Task<IActionResult> AddAmenity(int projectId, string name, string type, double? squareMeters, string description, bool isExisting = false)'
)
code = code.replace(
    'Description = description',
    'Description = description,\n                IsExisting = isExisting'
)

# Modify DeleteAmenity
code = code.replace(
    'public async Task<IActionResult> DeleteAmenity(int id)',
    'public async Task<IActionResult> DeleteAmenity(int id, bool isExisting = false)'
)
code = code.replace(
    'return RedirectToAction(nameof(Amenities), new { projectId = amenity.ConstructionProjectId });',
    'return RedirectToAction(nameof(Amenities), new { projectId = amenity.ConstructionProjectId, isExisting = isExisting });'
)
code = code.replace(
    'return RedirectToAction(nameof(Amenities), new { projectId = projectId });',
    'return RedirectToAction(nameof(Amenities), new { projectId = projectId, isExisting = isExisting });'
)

# Also in AddAmenity, change the return RedirectToAction
code = code.replace(
    'return RedirectToAction(nameof(Amenities), new { projectId = projectId });',
    'return RedirectToAction(nameof(Amenities), new { projectId = projectId, isExisting = isExisting });'
)


with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'w', encoding='utf-8') as f:
    f.write(code)

print("SUCCESS")
