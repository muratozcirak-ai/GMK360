$content = Get-Content -Raw "GMK360.Web\Controllers\ConstructionProjectController.cs"

$newAction = @"
        [HttpPost]
        public async Task<IActionResult> UpdateBlockSkeleton(int buildingId, int TotalFloors, int BasementFloors, bool HasGroundFloor = false, bool HasRoof = false)
        {
            var building = await _context.Buildings.FindAsync(buildingId);
            if (building != null)
            {
                building.TotalFloors = TotalFloors;
                building.BasementFloors = BasementFloors;
                building.HasGroundFloor = HasGroundFloor;
                building.HasRoof = HasRoof;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bina iskeleti (kat yapıları) başarıyla güncellendi.";
            }
            return RedirectToAction("ManageBlock", new { id = buildingId });
        }
"@

$content = $content.Replace("public async Task<IActionResult> ManageBlock", $newAction + "`r`n`r`n        public async Task<IActionResult> ManageBlock")

$content | Set-Content "GMK360.Web\Controllers\ConstructionProjectController.cs" -Encoding UTF8
Write-Output "Action added"
