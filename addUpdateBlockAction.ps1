$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
$text = [System.IO.File]::ReadAllText($path)

$actionCode = @"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBlockDetails(int Id, string BlockName, double? BaseArea, string FacadeDirection, string TechnicalFeatures, string Description)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var building = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .FirstOrDefaultAsync(b => b.Id == Id);

            if (building == null || building.ConstructionProject?.AgencyId != agencyId)
                return NotFound();

            building.BlockName = BlockName;
            building.BaseArea = BaseArea;
            building.FacadeDirection = FacadeDirection;
            building.TechnicalFeatures = TechnicalFeatures;
            building.Description = Description;

            _context.Update(building);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Blok özellikleri başarıyla güncellendi.";
            return RedirectToAction(nameof(ManageBlock), new { id = Id });
        }

"@

$text = $text.Replace("[HttpPost]`r`n        [ValidateAntiForgeryToken]`r`n        public async Task<IActionResult> UpdateUnitProperties", $actionCode + "        [HttpPost]`r`n        [ValidateAntiForgeryToken]`r`n        public async Task<IActionResult> UpdateUnitProperties")

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
