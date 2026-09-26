import re

filepath = r'GMK360.Web\Controllers\B2BPurchasingController.cs'
with open(filepath, 'r', encoding='latin1') as f:
    content = f.read()

old_signature = "public async Task<IActionResult> CreateQuote(string title, string description, DateTime? deadline, int? materialCatalogId, decimal? quantity)"
new_signature = "public async Task<IActionResult> CreateQuote(string title, string description, DateTime? deadline, int[] materialCatalogIds, decimal[] quantities)"

old_item_logic = """                if (materialCatalogId.HasValue && quantity.HasValue)
                {
                    var quoteItem = new B2BQuoteItem
                    {
                        B2BQuoteRequestId = quote.Id,
                        MaterialCatalogId = materialCatalogId.Value,
                        Quantity = quantity.Value,
                        Description = ""
                    };
                    _context.B2BQuoteItems.Add(quoteItem);
                    await _context.SaveChangesAsync();
                }"""

new_item_logic = """                if (materialCatalogIds != null && quantities != null)
                {
                    for(int i = 0; i < materialCatalogIds.Length; i++)
                    {
                        var catId = materialCatalogIds[i];
                        var qty = i < quantities.Length ? quantities[i] : 0;
                        
                        if (catId > 0 && qty > 0)
                        {
                            var quoteItem = new B2BQuoteItem
                            {
                                B2BQuoteRequestId = quote.Id,
                                MaterialCatalogId = catId,
                                Quantity = qty,
                                Description = ""
                            };
                            _context.B2BQuoteItems.Add(quoteItem);
                        }
                    }
                    await _context.SaveChangesAsync();
                }"""

# Replace title logic:
old_title_logic = """                string finalTitle = title;
                if (string.IsNullOrWhiteSpace(finalTitle) && materialCatalogId.HasValue)
                {
                    var cat = await _context.MaterialCatalogs.FindAsync(materialCatalogId.Value);
                    if (cat != null) finalTitle = $"{quantity} {cat.DefaultUnit} {cat.Name} Alýmý";
                }"""
                
new_title_logic = """                string finalTitle = title;
                if (string.IsNullOrWhiteSpace(finalTitle) && materialCatalogIds != null && materialCatalogIds.Length > 0)
                {
                    var firstCat = await _context.MaterialCatalogs.FindAsync(materialCatalogIds[0]);
                    if (firstCat != null) finalTitle = "Toplu Malzeme Alýmý (" + firstCat.Name + " vb.)";
                }"""

content = content.replace(old_signature, new_signature)
content = content.replace(old_item_logic, new_item_logic)
content = content.replace(old_title_logic, new_title_logic)

with open(filepath, 'w', encoding='latin1') as f:
    f.write(content)
print("Updated CreateQuote to handle arrays")
