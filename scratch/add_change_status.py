import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with open(filepath, 'r', encoding='latin1') as f:
    content = f.read()

# Add ChangeProjectStatus action
action_code = """
        [HttpPost]
        public async Task<IActionResult> ChangeProjectStatus(int id, int statusId)
        {
            var project = await _context.ConstructionProjects.FindAsync(id);
            if (project != null)
            {
                project.Status = (GMK360.Core.Entities.Construction.ProjectStatus)statusId;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Proje durumu başaryla güncellendi.";
            }
            return RedirectToAction("Details", new { id = id });
        }
"""

# inject right before the last closing brace. Or right before details action. Let's find "public async Task<IActionResult> Details"
if "public async Task<IActionResult> ChangeProjectStatus" not in content:
    content = content.replace("public async Task<IActionResult> Details", action_code + "\n        public async Task<IActionResult> Details")
    
with open(filepath, 'w', encoding='latin1') as f:
    f.write(content)
print("Added ChangeProjectStatus action")
