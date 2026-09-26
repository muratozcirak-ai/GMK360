import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

action_code = """
        [HttpGet]
        public async Task<IActionResult> BudgetDashboard(int id)
        {
            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == id);
            if(project == null) return NotFound();

            var budgetItems = await _context.ConstructionBudgetItems
                .Include(b => b.Supplier)
                .Where(b => b.ConstructionProjectId == id && !b.IsDeleted)
                .OrderBy(b => b.PhaseCategory)
                .ThenBy(b => b.CreatedAt)
                .ToListAsync();

            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectId = project.Id;

            return View(budgetItems);
        }
"""

# Insert before the last closing brace of the class
# (Just placing it near ManagePhases action is safer)

content = content.replace("public async Task<IActionResult> ManagePhases(int id)", action_code + "\n\n        [HttpGet]\n        public async Task<IActionResult> ManagePhases(int id)")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
