$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
$text = [System.IO.File]::ReadAllText($path)

$action = @"
        [HttpPost]
        public async Task<IActionResult> EditAmenity(int projectId, int amenityId, string name, double? squareMeters, string description)
        {
            var amenity = await _context.ProjectAmenities.FindAsync(amenityId);
            if (amenity != null)
            {
                amenity.Name = name;
                amenity.SquareMeters = squareMeters;
                amenity.Description = description;
                _context.Update(amenity);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Amenities), new { projectId = projectId });
        }
"@

$text = $text.Replace("public async Task<IActionResult> AddAmenity", $action + "`r`n        [HttpPost]`r`n        public async Task<IActionResult> AddAmenity")

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
