import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

post_action = """
        [HttpPost]
        public async Task<IActionResult> AddBudgetItem(int projectId, int phaseCategory, string itemName, string description, decimal quantity, string unit, decimal plannedUnitPrice, bool isExtra)
        {
            var item = new GMK360.Core.Entities.Construction.ConstructionBudgetItem
            {
                ConstructionProjectId = projectId,
                PhaseCategory = (GMK360.Core.Entities.Construction.BudgetPhaseCategory)phaseCategory,
                ItemName = itemName,
                Description = description,
                Quantity = quantity,
                Unit = unit,
                PlannedUnitPrice = plannedUnitPrice,
                ActualTotalCost = isExtra ? (quantity * plannedUnitPrice) : 0, // If extra, it might be actual directly, but let's keep it simple
                IsUnplannedExtra = isExtra,
                CreatedAt = DateTime.UtcNow,
                QuoteStatus = isExtra ? GMK360.Core.Entities.Construction.BudgetQuoteStatus.ActualInvoiced : GMK360.Core.Entities.Construction.BudgetQuoteStatus.EstimatedOrQuoted,
                SourceType = GMK360.Core.Entities.Construction.BudgetItemSourceType.Manual
            };

            // If it's an unplanned extra (like a receipt/taxi), we treat PlannedUnitPrice input as the Actual Cost for now.
            if (isExtra)
            {
                item.PlannedUnitPrice = 0;
                item.ActualTotalCost = plannedUnitPrice; // The input field will be repurposed in UI
            }

            _context.ConstructionBudgetItems.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToAction("BudgetDashboard", new { id = projectId });
        }
"""

if "AddBudgetItem" not in content:
    content = content.replace("public async Task<IActionResult> BudgetDashboard(int id)", post_action + "\n\n        [HttpGet]\n        public async Task<IActionResult> BudgetDashboard(int id)")
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
