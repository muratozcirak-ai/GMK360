import sys

filepath = 'GMK360.Web/Controllers/AdminController.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = """public IActionResult GlobalProviders()
        {
            return View();
        }"""
        
replacement = """public async Task<IActionResult> GlobalProviders()
        {
            var providers = await _context.B2bCompanies
                .Include(c => c.AddedByAgency)
                .Include(c => c.CompanyCategories)
                    .ThenInclude(cc => cc.B2bCategory)
                .Include(c => c.Branches)
                .Include(c => c.Contacts)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
                
            ViewBag.Categories = await _context.B2bCategories.ToListAsync();
            
            return View(providers);
        }"""

if "await _context.B2bCompanies" not in content:
    content = content.replace(target, replacement)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        print("AdminController updated.")
else:
    print("Already updated.")
