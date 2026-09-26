import re

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'r', encoding='utf-8') as f:
    code = f.read()

# For ToggleAmenityStatus
code = code.replace(
    'public async Task<IActionResult> ToggleAmenityStatus(int amenityId, int projectId)',
    'public async Task<IActionResult> ToggleAmenityStatus(int amenityId, int projectId, bool isExisting = false)'
)

# Wait, let's just make it generic. I'll replace ALL occurrences of 'isExisting = isExisting' with nothing if 'isExisting' isn't in the method signature?
# It's easier to just add ool isExisting = false to any method that redirects to Amenities!
# Let's find out which methods are redirecting to Amenities.
