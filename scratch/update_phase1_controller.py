import codecs
import re

path = 'GMK360.Web/Controllers/PhaseOneController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'// Sadece Fizibilite Aşaması İçin Listeyi Gönderiyoruz'
replacement = '''var templates = await _context.SystemPhaseTemplates.Where(t => t.PhaseCategory == BudgetPhaseCategory.YikimVeZeminHazirligi).ToListAsync();
            ViewData["HasTemplates"] = templates.Any();

            // Sadece Fizibilite Aşaması İçin Listeyi Gönderiyoruz'''
content = re.sub(target, replacement, content)

target2 = r'public async Task<IActionResult> DeleteItem\(int id\)'
replacement2 = '''[HttpPost("PhaseOne/SyncFromPool/{projectId}")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SyncFromPool(int projectId)
        {
            var existingItems = await _context.ConstructionBudgetItems
                .Where(b => b.ConstructionProjectId == projectId && b.PhaseCategory == BudgetPhaseCategory.YikimVeZeminHazirligi)
                .Select(b => b.ItemName)
                .ToListAsync();

            var templates = await _context.SystemPhaseTemplates
                .Where(t => t.PhaseCategory == BudgetPhaseCategory.YikimVeZeminHazirligi && !existingItems.Contains(t.ItemName))
                .ToListAsync();

            foreach(var t in templates)
            {
                var item = new ConstructionBudgetItem
                {
                    ConstructionProjectId = projectId,
                    PhaseCategory = BudgetPhaseCategory.YikimVeZeminHazirligi,
                    SubCategory = t.SubCategory,
                    ItemName = t.ItemName,
                    Quantity = 1,
                    Unit = "Adet",
                    SourceType = BudgetItemSourceType.SystemTemplate,
                    QuoteStatus = t.IsQuoteRequired ? BudgetQuoteStatus.WaitingForPrice : BudgetQuoteStatus.NoQuoteRequired
                };
                _context.ConstructionBudgetItems.Add(item);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { projectId = projectId });
        }

        [HttpPost("PhaseOne/DeleteItem/{id}")]'''

content = re.sub(target2, replacement2, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)