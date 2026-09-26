import re

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'r', encoding='utf-8') as f:
    code = f.read()

code = code.replace(
    'public async Task<IActionResult> UpdateLandArea(int projectId, double totalLandArea, double landscapeArea)',
    'public async Task<IActionResult> UpdateLandArea(int projectId, double totalLandArea, double landscapeArea, bool isExisting = false)'
)

code = code.replace(
    'public async Task<IActionResult> EditAmenity(int projectId, int amenityId, string name, double? squareMeters, string description)',
    'public async Task<IActionResult> EditAmenity(int projectId, int amenityId, string name, double? squareMeters, string description, bool isExisting = false)'
)

code = code.replace(
    'public async Task<IActionResult> DeleteAmenity(int amenityId, int projectId)',
    'public async Task<IActionResult> DeleteAmenity(int amenityId, int projectId, bool isExisting = false)'
)

code = code.replace(
    'public async Task<IActionResult> ToggleAmenityStatus(int amenityId, int projectId)',
    'public async Task<IActionResult> ToggleAmenityStatus(int amenityId, int projectId, bool isExisting = false)'
)

# And in AddAmenity I need to ensure IsExisting is set
code = code.replace(
    'Description = description\n            };',
    'Description = description,\n                IsExisting = isExisting\n            };'
)


with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'w', encoding='utf-8') as f:
    f.write(code)

print("SUCCESS")
