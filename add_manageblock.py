import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

new_methods = """
        // GET: ConstructionProject/ManageBlock/5
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
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateUnits(int buildingId, int floors, int unitsPerFloor, string roomLayout)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var block = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (block == null) return NotFound();

            if (!User.IsInRole("Admin") && block.ConstructionProject.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            int doorCounter = 1;
            for (int f = 0; f < floors; f++)
            {
                for (int u = 0; u < unitsPerFloor; u++)
                {
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = doorCounter.ToString(),
                        RoomLayout = string.IsNullOrEmpty(roomLayout) ? "3+1" : roomLayout,
                        IsEmpty = true
                    };
                    _context.BuildingUnits.Add(unit);
                    doorCounter++;
                }
            }
            
            block.TotalFloors = floors;
            block.TotalUnits = floors * unitsPerFloor;
            
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{floors * unitsPerFloor} adet bağımsız bölüm başarıyla oluşturuldu.";
            return RedirectToAction("ManageBlock", new { id = buildingId });
        }
"""

# Insert these methods before the Edit method
content = content.replace('// GET: ConstructionProject/Edit/5', new_methods + '\n        // GET: ConstructionProject/Edit/5')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Added ManageBlock and GenerateUnits to controller.")
