import codecs
import re

path = 'GMK360.Web/Controllers/PhaseOneController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'\[HttpPost\("PhaseOne/UpdatePrice"\)\]\s*\[IgnoreAntiforgeryToken\]\s*public async Task<IActionResult> UpdatePrice\(int id, decimal plannedUnitPrice, string description\)\s*\{\s*var item = await _context\.ConstructionBudgetItems\.FindAsync\(id\);\s*if \(item == null\) return NotFound\(\);\s*item\.PlannedUnitPrice = plannedUnitPrice;\s*item\.Description = description;\s*item\.QuoteStatus = BudgetQuoteStatus\.EstimatedOrQuoted;\s*await _context\.SaveChangesAsync\(\);\s*return RedirectToAction\("Index", new \{ projectId = item\.ConstructionProjectId \}\);\s*\}'
replacement = '''[HttpPost("PhaseOne/UpdatePrice")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdatePrice(int id, decimal totalCost, decimal quantity, string unit, string description)
        {
            var item = await _context.ConstructionBudgetItems.FindAsync(id);
            if (item == null) return NotFound();

            item.Quantity = quantity > 0 ? quantity : 1;
            item.Unit = string.IsNullOrEmpty(unit) ? "Götürü" : unit;
            item.PlannedUnitPrice = totalCost / item.Quantity; // Calculate unit price from total
            
            item.Description = description;
            item.QuoteStatus = BudgetQuoteStatus.EstimatedOrQuoted;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { projectId = item.ConstructionProjectId });
        }'''
content = re.sub(target, replacement, content)

target_add = r'\[HttpPost\("PhaseOne/AddItem"\)\]\s*\[IgnoreAntiforgeryToken\]\s*public async Task<IActionResult> AddItem\(int projectId, string subCategory, string itemName, decimal plannedUnitPrice\)\s*\{\s*var item = new ConstructionBudgetItem\s*\{\s*ConstructionProjectId = projectId,\s*PhaseCategory = BudgetPhaseCategory\.YikimVeZeminHazirligi,\s*SubCategory = subCategory,\s*ItemName = itemName,\s*PlannedUnitPrice = plannedUnitPrice,\s*SourceType = BudgetItemSourceType\.Manual,\s*QuoteStatus = plannedUnitPrice > 0 \? BudgetQuoteStatus\.EstimatedOrQuoted : BudgetQuoteStatus\.WaitingForPrice\s*\};'
replacement_add = '''[HttpPost("PhaseOne/AddItem")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddItem(int projectId, string subCategory, string itemName, decimal totalCost, decimal quantity, string unit)
        {
            decimal qty = quantity > 0 ? quantity : 1;
            var item = new ConstructionBudgetItem
            {
                ConstructionProjectId = projectId,
                PhaseCategory = BudgetPhaseCategory.YikimVeZeminHazirligi,
                SubCategory = subCategory,
                ItemName = itemName,
                Quantity = qty,
                Unit = string.IsNullOrEmpty(unit) ? "Götürü" : unit,
                PlannedUnitPrice = totalCost / qty,
                SourceType = BudgetItemSourceType.Manual,
                QuoteStatus = totalCost > 0 ? BudgetQuoteStatus.EstimatedOrQuoted : BudgetQuoteStatus.WaitingForPrice
            };'''
content = re.sub(target_add, replacement_add, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)