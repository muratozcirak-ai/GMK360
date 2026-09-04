$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
$text = [System.IO.File]::ReadAllText($path)

$amenitiesActions = @"
        // --- SOSYAL DONATILAR VE DIŞ ALANLAR (AMENITIES) ---
        public async Task<IActionResult> Amenities(int projectId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var project = await _context.ConstructionProjects
                .Include(p => p.Amenities)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null || project.AgencyId != agencyId)
                return NotFound();

            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> AddAmenity(int projectId, string name, string type, double? squareMeters, string description)
        {
            var amenity = new GMK360.Core.Entities.Construction.ProjectAmenity
            {
                ConstructionProjectId = projectId,
                Name = name,
                Type = type,
                SquareMeters = squareMeters,
                Description = description
            };

            _context.ProjectAmenities.Add(amenity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yeni Sosyal Donatı / Açık Alan eklendi.";
            return RedirectToAction(nameof(Amenities), new { projectId = projectId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAmenity(int amenityId, int projectId)
        {
            var amenity = await _context.ProjectAmenities.FindAsync(amenityId);
            if (amenity != null)
            {
                _context.ProjectAmenities.Remove(amenity);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Açık alan başarıyla silindi.";
            }
            return RedirectToAction(nameof(Amenities), new { projectId = projectId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAmenityStatus(int amenityId, int projectId)
        {
            var amenity = await _context.ProjectAmenities.FindAsync(amenityId);
            if (amenity != null)
            {
                amenity.IsCompleted = !amenity.IsCompleted;
                _context.ProjectAmenities.Update(amenity);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Amenities), new { projectId = projectId });
        }
"@

# Insert before // --- DAIRE TİPLERİ (ŞABLONLAR) ---
$text = $text.Replace("// --- DAIRE TİPLERİ (ŞABLONLAR) ---", $amenitiesActions + "`r`n`r`n        // --- DAIRE TİPLERİ (ŞABLONLAR) ---")
[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
