import sys
import re

filepath = 'GMK360.Web/Controllers/AdminController.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = "ViewBag.Categories = await _context.B2bCategories.ToListAsync();"
replacement = """ViewBag.Categories = await _context.DefinitionValues
                .Include(v => v.Category)
                .Where(v => v.Category.SystemCode == "B2BSectors")
                .OrderBy(v => v.Order)
                .ToListAsync();
                
            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();"""

if "await _context.Cities.OrderBy" not in content:
    content = content.replace(target, replacement)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        print("AdminController updated for Definitions and Cities.")
else:
    print("Already updated.")
