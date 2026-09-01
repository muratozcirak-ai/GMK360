import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

old_manage_block = """        // GET: ConstructionProject/ManageBlock/5
        public async Task<IActionResult> ManageBlock(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var block = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .Include(b => b.Units)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (block == null) return NotFound();
            
            if (!User.IsInRole("Admin") && block.ConstructionProject.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            return View(block);
        }"""

new_manage_block = """        // GET: ConstructionProject/ManageBlock/5
        public async Task<IActionResult> ManageBlock(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var block = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .Include(b => b.Units)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (block == null) return NotFound();
            
            if (!User.IsInRole("Admin") && block.ConstructionProject.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            // Mimari görselleri ve kat planlarını getir
            var architectureDocs = await _context.DmsDocuments
                .Where(d => (d.EntityType == "Building" && d.EntityId == id.ToString()) ||
                            (d.EntityType == "BuildingFloor" && d.EntityId.StartsWith(id.ToString() + "_")))
                .ToListAsync();

            ViewBag.BlockImages = architectureDocs.Where(d => d.EntityType == "Building").ToList();
            ViewBag.FloorPlans = architectureDocs.Where(d => d.EntityType == "BuildingFloor").ToList();

            return View(block);
        }"""

content = content.replace(old_manage_block, new_manage_block)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated ManageBlock action with ViewBag docs.")
