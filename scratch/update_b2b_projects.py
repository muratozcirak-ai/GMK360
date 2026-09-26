import re

filepath = r'GMK360.Web\Controllers\B2BPurchasingController.cs'
with open(filepath, 'r', encoding='latin1') as f:
    content = f.read()

# I will add the ViewBag.Projects query before "return View(quotes);"
old_logic = "if (agencyId.HasValue) ViewBag.Catalogs = await _context.MaterialCatalogs.Where(c => c.AgencyId == agencyId.Value).OrderBy(c => c.Name).ToListAsync();\n                return View(quotes);"

new_logic = """if (agencyId.HasValue) 
                {
                    ViewBag.Catalogs = await _context.MaterialCatalogs.Where(c => c.AgencyId == agencyId.Value).OrderBy(c => c.Name).ToListAsync();
                    ViewBag.Projects = await _context.ConstructionProjects.Where(p => p.AgencyId == agencyId.Value && p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Aktif_Santiye).OrderBy(p => p.Name).ToListAsync();
                }
                return View(quotes);"""

content = content.replace(old_logic, new_logic)

with open(filepath, 'w', encoding='latin1') as f:
    f.write(content)
print("Updated Index for ViewBag.Projects")
