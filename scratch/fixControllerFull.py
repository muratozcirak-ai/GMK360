import re

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'r', encoding='utf-8') as f:
    code = f.read()

# 1. SaveStep2 bug
code = code.replace(
    'existingBlocks.FirstOrDefault(eb => eb.BlockName == b.BlockName)',
    'existingBlocks.FirstOrDefault(eb => eb.BlockName == b.BlockName && eb.IsExistingBuilding == b.IsExistingBuilding)'
)

# 2. Details image fetch
pattern = re.compile(r'(public async Task<IActionResult> Details\(int\? id\).*?)(var project = await _context\.ConstructionProjects\s*\.Include)', re.DOTALL)
insertion = '''
            // Fetch images from DocumentArchive dynamically if they exist (in case user uploaded manually)
            var sahaGorseli = await _context.DocumentArchives
                .Where(d => d.SourceModule == "Construction" && d.ProjectId == id && d.Category == "Saha Görseli")
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();
            if (sahaGorseli != null) {
                ViewBag.CurrentStateImageUrl = sahaGorseli.DocumentUrl;
            }
            
            var projeGorseli = await _context.DocumentArchives
                .Where(d => d.SourceModule == "Construction" && d.ProjectId == id && (d.Category == "Bina Görselleri" || d.Category == "Proje Görseli"))
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();
            if (projeGorseli != null) {
                ViewBag.CoverImageUrl = projeGorseli.DocumentUrl;
            }
            
            '''
match = pattern.search(code)
if match:
    code = code[:match.end(1)] + insertion + match.group(2) + code[match.end(2):]

# 3. Amenities logic (VERY CAREFULLY)

# Amenities GET
code = code.replace(
    'public async Task<IActionResult> Amenities(int projectId)',
    'public async Task<IActionResult> Amenities(int projectId, bool isExisting = false)'
)
code = code.replace(
    'var project = await _context.ConstructionProjects\n                .Include(p => p.Amenities)',
    'ViewBag.IsExisting = isExisting;\n\n            var project = await _context.ConstructionProjects\n                .Include(p => p.Amenities)'
)

# AddAmenity
code = code.replace(
    'public async Task<IActionResult> AddAmenity(int projectId, string name, string type, double? squareMeters, string description)',
    'public async Task<IActionResult> AddAmenity(int projectId, string name, string type, double? squareMeters, string description, bool isExisting = false)'
)
add_amenity_find = '''var amenity = new GMK360.Core.Entities.Construction.ProjectAmenity
            {
                ConstructionProjectId = projectId,
                Name = name,
                Type = type,
                SquareMeters = squareMeters,
                Description = description
            };'''
add_amenity_replace = '''var amenity = new GMK360.Core.Entities.Construction.ProjectAmenity
            {
                ConstructionProjectId = projectId,
                Name = name,
                Type = type,
                SquareMeters = squareMeters,
                Description = description,
                IsExisting = isExisting
            };'''
code = code.replace(add_amenity_find, add_amenity_replace)

code = code.replace(
    'return RedirectToAction(nameof(Amenities), new { projectId = projectId });',
    'return RedirectToAction(nameof(Amenities), new { projectId = projectId, isExisting = isExisting });'
)

# DeleteAmenity
code = code.replace(
    'public async Task<IActionResult> DeleteAmenity(int id)',
    'public async Task<IActionResult> DeleteAmenity(int id, bool isExisting = false)'
)
code = code.replace(
    'return RedirectToAction(nameof(Amenities), new { projectId = amenity.ConstructionProjectId });',
    'return RedirectToAction(nameof(Amenities), new { projectId = amenity.ConstructionProjectId, isExisting = isExisting });'
)

# EditAmenity
code = code.replace(
    'public async Task<IActionResult> EditAmenity(int amenityId, int projectId, string name, double? squareMeters, string description)',
    'public async Task<IActionResult> EditAmenity(int amenityId, int projectId, string name, double? squareMeters, string description, bool isExisting = false)'
)

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'w', encoding='utf-8') as f:
    f.write(code)

print("SUCCESS")
