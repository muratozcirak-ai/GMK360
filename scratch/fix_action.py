import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

old_action = """        [HttpPost]
        public async Task<IActionResult> ChangeProjectStatus(int id, int statusId)
        {
            var project = await _context.ConstructionProjects.FindAsync(id);
            if (project != null)
            {
                project.Status = (GMK360.Core.Entities.Construction.ProjectStatus)statusId;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Proje durumu başarıyla güncellendi.";
            }
            return RedirectToAction("Details", new { id = id });
        }"""

new_action = """        [HttpPost]
        public async Task<IActionResult> ChangeProjectStatus(int id, string newStatus)
        {
            var project = await _context.ConstructionProjects.FindAsync(id);
            if (project != null && Enum.TryParse(typeof(GMK360.Core.Entities.Construction.ProjectStatus), newStatus, out var statusObj))
            {
                project.Status = (GMK360.Core.Entities.Construction.ProjectStatus)statusObj;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Proje durumu başarıyla güncellendi.";
            }
            return RedirectToAction("Details", new { id = id });
        }"""

content = content.replace(old_action, new_action)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Done")
