$ctrlPath = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
$ctrlText = [System.IO.File]::ReadAllText($ctrlPath)

$newMethods = @"
        // --- DAIRE ICI ALAN YONETIMI ---
        public async Task<IActionResult> ManageUnit(int unitId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                    .ThenInclude(b => b.ConstructionProject)
                .Include(u => u.Spaces)
                .FirstOrDefaultAsync(u => u.Id == unitId);

            if (unit == null) return NotFound();

            if (!User.IsInRole("Admin") && unit.Building.ConstructionProject.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            return View(unit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUnitSpace(int unitId, string name, string spaceType, double? squareMeters)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                    .ThenInclude(b => b.ConstructionProject)
                .FirstOrDefaultAsync(u => u.Id == unitId);

            if (unit == null || (!User.IsInRole("Admin") && unit.Building.ConstructionProject.AgencyId != agencyId))
                return Unauthorized();

            var space = new GMK360.Core.Entities.UnitSpace
            {
                BuildingUnitId = unitId,
                Name = name,
                Type = spaceType,
                SquareMeters = squareMeters
            };

            _context.UnitSpaces.Add(space);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageUnit), new { unitId = unitId });
        }
"@

$regex = [regex]::new("}\s*}", [System.Text.RegularExpressions.RegexOptions]::RightToLeft)
$ctrlText = $regex.Replace($ctrlText, $newMethods + "`r`n    }`r`n}", 1)

[System.IO.File]::WriteAllText($ctrlPath, $ctrlText, [System.Text.Encoding]::UTF8)
