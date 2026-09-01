import re
import os

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace GenerateUnits method
old_method = """        [HttpPost]
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
        }"""

new_method = """        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateUnits(int buildingId, int normalFloors, int basementFloors, bool hasGroundFloor, int unitsPerFloor, string roomLayout)
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
            int totalUnitsCreated = 0;

            // 1. Bodrum Katları (Örn: -2, -1)
            for (int f = basementFloors; f >= 1; f--)
            {
                for (int u = 0; u < unitsPerFloor; u++)
                {
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = doorCounter.ToString(),
                        FloorLevel = -f,
                        FloorName = $"-{f}. Kat (Bodrum)",
                        RoomLayout = string.IsNullOrEmpty(roomLayout) ? "3+1" : roomLayout,
                        IsEmpty = true
                    };
                    _context.BuildingUnits.Add(unit);
                    doorCounter++;
                    totalUnitsCreated++;
                }
            }

            // 2. Zemin Kat (0)
            if (hasGroundFloor)
            {
                for (int u = 0; u < unitsPerFloor; u++)
                {
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = doorCounter.ToString(),
                        FloorLevel = 0,
                        FloorName = "Zemin Kat",
                        RoomLayout = string.IsNullOrEmpty(roomLayout) ? "3+1" : roomLayout,
                        IsEmpty = true
                    };
                    _context.BuildingUnits.Add(unit);
                    doorCounter++;
                    totalUnitsCreated++;
                }
            }

            // 3. Normal Katlar (1, 2, 3...)
            for (int f = 1; f <= normalFloors; f++)
            {
                for (int u = 0; u < unitsPerFloor; u++)
                {
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = doorCounter.ToString(),
                        FloorLevel = f,
                        FloorName = $"{f}. Kat",
                        RoomLayout = string.IsNullOrEmpty(roomLayout) ? "3+1" : roomLayout,
                        IsEmpty = true
                    };
                    _context.BuildingUnits.Add(unit);
                    doorCounter++;
                    totalUnitsCreated++;
                }
            }
            
            block.TotalFloors = normalFloors;
            block.BasementFloors = basementFloors;
            block.HasGroundFloor = hasGroundFloor;
            block.TotalUnits = (block.TotalUnits == 0) ? totalUnitsCreated : block.TotalUnits + totalUnitsCreated; // Varsa üstüne ekle
            
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{totalUnitsCreated} adet bağımsız bölüm başarıyla oluşturuldu.";
            return RedirectToAction("ManageBlock", new { id = buildingId });
        }"""

if old_method in content:
    content = content.replace(old_method, new_method)
else:
    print("WARNING: Could not find exactly old_method text. Using regex.")
    content = re.sub(r'public async Task<IActionResult> GenerateUnits\(.*?\).*?return RedirectToAction\("ManageBlock", new \{ id = buildingId \}\);\s*\}', new_method, content, flags=re.DOTALL)


with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated GenerateUnits method.")
