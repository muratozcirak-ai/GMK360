$ctrlPath = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
$ctrlText = [System.IO.File]::ReadAllText($ctrlPath)

$oldMethodStart = "public async Task<IActionResult> GenerateUnits(int buildingId, int normalFloors, int basementFloors, bool hasGroundFloor, int unitsPerFloor, string roomLayout)"
$oldMethodRegex = [regex]::Escape($oldMethodStart) + "[\s\S]*?(?=\s+public async Task<IActionResult>|\s*// --- DAIRE ICI ALAN YONETIMI ---)"

$newMethod = @"
        public async Task<IActionResult> GenerateUnits(int buildingId, int normalFloors, int basementFloors, bool hasGroundFloor, int unitsPerFloor, string roomLayout)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var block = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .Include(b => b.Units)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (block == null) return NotFound();

            if (!User.IsInRole("Admin") && block.ConstructionProject.AgencyId != agencyId)
            {
                return Unauthorized();
            }
            
            // 0. ESKI BIRIMLERI TEMIZLE
            if (block.Units != null && block.Units.Any())
            {
                _context.BuildingUnits.RemoveRange(block.Units);
                await _context.SaveChangesAsync();
            }

            int doorCounter = 1;
            int totalUnitsCreated = 0;

            // 1. Bodrum Katlar (1 birim Ortak Alan - Otopark/Sığınak)
            for (int f = basementFloors; f >= 1; f--)
            {
                var unit = new GMK360.Core.Entities.BuildingUnit
                {
                    BuildingId = buildingId,
                    DoorNumber = (f == 1) ? "Sığınak" : $"Otopark (-{f})",
                    FloorLevel = -f,
                    FloorName = $"-{f}. Kat (Bodrum)",
                    RoomLayout = "Ortak Alan",
                    IsEmpty = true
                };
                _context.BuildingUnits.Add(unit);
                totalUnitsCreated++;
            }

            // 2. Zemin Kat (Dükkan / Ticari)
            if (hasGroundFloor)
            {
                for (int u = 0; u < unitsPerFloor; u++)
                {
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = $"Dükkan {doorCounter}",
                        FloorLevel = 0,
                        FloorName = "Zemin Kat",
                        RoomLayout = "Ticari Alan",
                        IsEmpty = true
                    };
                    _context.BuildingUnits.Add(unit);
                    doorCounter++;
                    totalUnitsCreated++;
                }
            }

            // 3. Normal Katlar (Standart Daire Planı)
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

            _context.Buildings.Update(block);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Eski kayıtlar temizlendi. Toplam {totalUnitsCreated} bağımsız bölüm akıllı mantığa göre başarıyla üretildi.";
            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
        }
"@

$ctrlText = [System.Text.RegularExpressions.Regex]::Replace($ctrlText, $oldMethodRegex, $newMethod)
[System.IO.File]::WriteAllText($ctrlPath, $ctrlText, [System.Text.Encoding]::UTF8)

