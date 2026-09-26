import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Remove the faulty AddBudgetItem entirely.
# Let's locate it:
pattern = r"\[HttpPost\]\s*public async Task<IActionResult> AddBudgetItem\(int projectId, string title, decimal amount, string category, string receiptUrl\).*?return RedirectToAction\(nameof\(BudgetDashboard\), new \{ id = projectId \}\);\s*\}"

replacement = """
        [HttpPost]
        public async Task<IActionResult> AddBudgetItem(int projectId, string itemName, decimal amount, string description)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var item = new GMK360.Core.Entities.Construction.ConstructionBudgetItem
            {
                ConstructionProjectId = projectId,
                ItemName = itemName ?? "Masraf",
                ActualTotalCost = amount,
                Description = description,
                QuoteStatus = GMK360.Core.Entities.Construction.BudgetQuoteStatus.ActualInvoiced,
                IsUnplannedExtra = true,
                CreatedAt = System.DateTime.UtcNow,
                IsDeleted = false
            };
            
            _context.ConstructionBudgetItems.Add(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Fiş/Gider başarıyla bütçeye eklendi.";
            return RedirectToAction(nameof(BudgetDashboard), new { id = projectId });
        }
"""
content = re.sub(pattern, replacement, content, flags=re.DOTALL)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("AddBudgetItem fixed in python.")
