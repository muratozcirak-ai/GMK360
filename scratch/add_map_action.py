import re

with open(r'GMK360.Web\Controllers\ConstructionProjectController.cs', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

target = 'public async Task<IActionResult> Details(int? id)'
new_action = '''[HttpGet]
        public async Task<IActionResult> Map(int id)
        {
            var project = await _context.ConstructionProjects.FindAsync(id);
            if (project != null && project.Latitude.HasValue && project.Longitude.HasValue)
            {
                return Redirect($"https://www.google.com/maps?q={project.Latitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)},{project.Longitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            }
            TempData["ErrorMessage"] = "Şantiye konumu henüz harita üzerinde işaretlenmemiş.";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        '''

if new_action not in content:
    content = content.replace(target, new_action + target)

with open(r'GMK360.Web\Controllers\ConstructionProjectController.cs', 'w', encoding='utf-8-sig') as f:
    f.write(content)
print("done")
