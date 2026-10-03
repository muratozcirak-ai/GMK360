import codecs
import re

path = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

method = '''
        [HttpPost]
        public async Task<IActionResult> SyncAllPhases(int projectId)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();

            var templates = await _context.SystemPhaseTemplates
                .Where(t => !t.IsDeleted)
                .ToListAsync();

            var existingItems = await _context.ConstructionBudgets
                .Where(b => b.ProjectId == projectId)
                .ToListAsync();

            var newItems = new List<ConstructionBudgetItem>();

            foreach (var template in templates)
            {
                bool exists = existingItems.Any(e => 
                    e.PhaseCategory == template.PhaseCategory && 
                    e.SubCategory == template.SubCategory && 
                    e.ItemName == template.ItemName);

                if (!exists)
                {
                    newItems.Add(new ConstructionBudgetItem
                    {
                        ConstructionProjectId = projectId,
                        ProjectId = projectId,
                        PhaseCategory = template.PhaseCategory,
                        SubCategory = template.SubCategory,
                        ItemName = template.ItemName,
                        Quantity = 1,
                        Unit = "Adet",
                        PlannedUnitPrice = 0,
                        IsQuoteRequired = false
                    });
                }
            }

            if (newItems.Any())
            {
                _context.ConstructionBudgets.AddRange(newItems);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"Başarıyla {newItems.Count} kalem havuza göre senkronize edildi!";
            return RedirectToAction("Details", new { id = projectId });
        }
'''

# Find the last closing brace of the class and insert the method
match = re.search(r'}\s*}\s*$', content)
if match:
    content = content[:match.start()] + method + '\n    }\n}\n'

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)