$ctrlPath = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
$ctrlText = [System.IO.File]::ReadAllText($ctrlPath)

$helperMethod = @"
        private List<GMK360.Core.Entities.UnitSpace> GetDefaultSpacesForLayout(string layout)
        {
            var spaces = new List<GMK360.Core.Entities.UnitSpace>();

            if (layout == "3+1")
            {
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Salon", Type = "Yaşam Alanı" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Mutfak", Type = "Mutfak" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Banyo", Type = "Islak Hacim" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Ebeveyn Banyosu", Type = "Islak Hacim" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Yatak Odası 1", Type = "Yaşam Alanı" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Yatak Odası 2", Type = "Yaşam Alanı" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Yatak Odası 3", Type = "Yaşam Alanı" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Balkon 1", Type = "Balkon" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Balkon 2", Type = "Balkon" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Antre / Hol", Type = "Sirkülasyon" });
            }
            else if (layout == "2+1")
            {
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Salon", Type = "Yaşam Alanı" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Mutfak", Type = "Mutfak" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Banyo", Type = "Islak Hacim" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Yatak Odası 1", Type = "Yaşam Alanı" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Yatak Odası 2", Type = "Yaşam Alanı" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Balkon", Type = "Balkon" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Antre / Hol", Type = "Sirkülasyon" });
            }
            else if (layout == "1+1")
            {
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Salon + Amerikan Mutfak", Type = "Yaşam Alanı" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Banyo", Type = "Islak Hacim" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Yatak Odası", Type = "Yaşam Alanı" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Balkon", Type = "Balkon" });
            }
            else if (layout == "4+1")
            {
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Salon", Type = "Yaşam Alanı" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Mutfak", Type = "Mutfak" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Banyo", Type = "Islak Hacim" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Ebeveyn Banyosu", Type = "Islak Hacim" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Giyinme Odası", Type = "Depolama" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Yatak Odası 1", Type = "Yaşam Alanı" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Yatak Odası 2", Type = "Yaşam Alanı" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Yatak Odası 3", Type = "Yaşam Alanı" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Yatak Odası 4", Type = "Yaşam Alanı" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Balkon 1", Type = "Balkon" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Balkon 2", Type = "Balkon" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Antre / Hol", Type = "Sirkülasyon" });
            }
            else if (layout == "Ticari Alan")
            {
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Ana Satış Alanı", Type = "Yaşam Alanı" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "WC", Type = "Islak Hacim" });
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Depo", Type = "Depolama" });
            }
            else if (layout == "Ortak Alan")
            {
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = "Ana Kullanım Alanı", Type = "Ortak Alan" });
            }

            return spaces;
        }

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
                var layout = "Ortak Alan";
                var unit = new GMK360.Core.Entities.BuildingUnit
                {
                    BuildingId = buildingId,
                    DoorNumber = (f == 1) ? "Sığınak" : $"Otopark (-{f})",
                    FloorLevel = -f,
                    FloorName = $"-{f}. Kat (Bodrum)",
                    RoomLayout = layout,
                    IsEmpty = true,
                    Spaces = GetDefaultSpacesForLayout(layout)
                };
                _context.BuildingUnits.Add(unit);
                totalUnitsCreated++;
            }

            // 2. Zemin Kat (Dükkan / Ticari)
            if (hasGroundFloor)
            {
                for (int u = 0; u < unitsPerFloor; u++)
                {
                    var layout = "Ticari Alan";
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = $"Dükkan {doorCounter}",
                        FloorLevel = 0,
                        FloorName = "Zemin Kat",
                        RoomLayout = layout,
                        IsEmpty = true,
                        Spaces = GetDefaultSpacesForLayout(layout)
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
                    var layout = string.IsNullOrEmpty(roomLayout) ? "3+1" : roomLayout;
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = doorCounter.ToString(),
                        FloorLevel = f,
                        FloorName = $"{f}. Kat",
                        RoomLayout = layout,
                        IsEmpty = true,
                        Spaces = GetDefaultSpacesForLayout(layout)
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

            TempData["SuccessMessage"] = $"Eski kayıtlar temizlendi. Seçilen {roomLayout} planına göre iç odalar otomatik yerleştirilerek toplam {totalUnitsCreated} bağımsız bölüm üretildi.";
            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
        }
"@

$oldMethodStart = "public async Task<IActionResult> GenerateUnits(int buildingId, int normalFloors, int basementFloors, bool hasGroundFloor, int unitsPerFloor, string roomLayout)"
$oldMethodRegex = [regex]::Escape($oldMethodStart) + "[\s\S]*?(?=\s+public async Task<IActionResult>|\s*// --- DAIRE ICI ALAN YONETIMI ---)"

$ctrlText = [System.Text.RegularExpressions.Regex]::Replace($ctrlText, $oldMethodRegex, $helperMethod)
[System.IO.File]::WriteAllText($ctrlPath, $ctrlText, [System.Text.Encoding]::UTF8)
