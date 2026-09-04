$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
$text = [System.IO.File]::ReadAllText($path)

$oldAction = @"
        public async Task<IActionResult> UpdateBlockDetails(int Id, string BlockName, double? BaseArea, string FacadeDirection, string TechnicalFeatures, string Description, string InsulationType, int? ElevatorCount, string ParkingType)
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

            return RedirectToAction(nameof(ManageBlock), new { projectId = building.ConstructionProjectId, blockId = building.Id });
        }
"@

$newAction = @"
        public async Task<IActionResult> UpdateBlockDetails(int Id, string BlockName, double? BaseArea, List<string> SelectedFeatures, string TechnicalFeatures, string Description, string InsulationType, int? ElevatorCount, string ParkingType)
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
            
            // Combine predefined checkboxes into a comma-separated string
            string combinedFeatures = """";
            if (SelectedFeatures != null && SelectedFeatures.Any())
            {
                combinedFeatures = string.Join("", "", SelectedFeatures);
            }
            
            // If they also typed custom technical features, append them
            if (!string.IsNullOrEmpty(TechnicalFeatures))
            {
                if (!string.IsNullOrEmpty(combinedFeatures)) combinedFeatures += "", "";
                combinedFeatures += TechnicalFeatures;
            }
            
            building.TechnicalFeatures = combinedFeatures;
            
            building.Description = Description;
            building.InsulationType = InsulationType;
            building.ElevatorCount = ElevatorCount;
            building.ParkingType = ParkingType;

            _context.Update(building);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageBlock), new { projectId = building.ConstructionProjectId, blockId = building.Id });
        }
"@

$text = $text.Replace($oldAction, $newAction)
[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
