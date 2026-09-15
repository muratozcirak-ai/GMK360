import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\InventoryController.cs'
with codecs.open(filepath, 'r', 'utf-8') as f:
    content = f.read()

index_old = '''public async Task<IActionResult> Index()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null && !User.IsInRole("Admin")) return Unauthorized();

            var query = _context.Warehouses.Include(w => w.Items).AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(w => w.AgencyId == agencyId);
            }

            var warehouses = await query.ToListAsync();
            return View(warehouses);
        }'''

index_new = '''public async Task<IActionResult> Index(int? id) // id is projectId
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null && !User.IsInRole("Admin")) return Unauthorized();

            var query = _context.Warehouses.Include(w => w.Items).AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(w => w.AgencyId == agencyId);
            }
            
            if (id.HasValue && id.Value > 0)
            {
                query = query.Where(w => w.ConstructionProjectId == id.Value);
                ViewData["ProjectName"] = await _context.ConstructionProjects.Where(p => p.Id == id).Select(p => p.Name).FirstOrDefaultAsync();
                ViewData["ProjectId"] = id;
            }

            var warehouses = await query.ToListAsync();
            return View(warehouses);
        }'''

content = content.replace(index_old, index_new)

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
