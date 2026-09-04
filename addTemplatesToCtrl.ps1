$ctrlPath = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
$ctrlText = [System.IO.File]::ReadAllText($ctrlPath)

$newActions = @"
        // --- DAIRE TİPLERİ (ŞABLONLAR) ---
        
        public async Task<IActionResult> Templates(int projectId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var project = await _context.ConstructionProjects
                .Include(p => p.UnitTemplates)
                .ThenInclude(t => t.Spaces)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return NotFound();
            if (!User.IsInRole("Admin") && project.AgencyId != agencyId) return Unauthorized();

            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> AddTemplate(int projectId, string name, string roomLayout, string description)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var template = new GMK360.Core.Entities.UnitTemplate
            {
                ConstructionProjectId = projectId,
                Name = name,
                RoomLayout = roomLayout,
                Description = description
            };

            _context.UnitTemplates.Add(template);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yeni Daire Tipi Şablonu başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Templates), new { projectId = projectId });
        }

        public async Task<IActionResult> TemplateSpaces(int templateId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var template = await _context.UnitTemplates
                .Include(t => t.ConstructionProject)
                .Include(t => t.Spaces)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null) return NotFound();
            if (!User.IsInRole("Admin") && template.ConstructionProject.AgencyId != agencyId) return Unauthorized();

            return View(template);
        }

        [HttpPost]
        public async Task<IActionResult> AddTemplateSpace(int templateId, string name, string type, double? squareMeters)
        {
            var space = new GMK360.Core.Entities.UnitTemplateSpace
            {
                UnitTemplateId = templateId,
                Name = name,
                Type = type,
                SquareMeters = squareMeters
            };

            _context.UnitTemplateSpaces.Add(space);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Şablona yeni alan eklendi.";
            return RedirectToAction(nameof(TemplateSpaces), new { templateId = templateId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTemplateSpace(int spaceId, int templateId)
        {
            var space = await _context.UnitTemplateSpaces.FindAsync(spaceId);
            if (space != null)
            {
                _context.UnitTemplateSpaces.Remove(space);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Alan şablondan silindi.";
            }
            return RedirectToAction(nameof(TemplateSpaces), new { templateId = templateId });
        }
"@

# Inject after ManageBlock
$pattern = "(public async Task<IActionResult> ManageBlock\(int id\)[\s\S]*?return View\(block\);\s*})"
$ctrlText = [System.Text.RegularExpressions.Regex]::Replace($ctrlText, $pattern, "`$1`n`n$newActions")

# Update GenerateUnits
$oldGenStart = "public async Task<IActionResult> GenerateUnits\(int buildingId, int normalFloors, int basementFloors, bool hasGroundFloor, int unitsPerFloor, string roomLayout\)"
$newGenStart = "public async Task<IActionResult> GenerateUnits(int buildingId, int normalFloors, int basementFloors, bool hasGroundFloor, int unitsPerFloor, int? templateId)"

$ctrlText = [System.Text.RegularExpressions.Regex]::Replace($ctrlText, $oldGenStart, $newGenStart)

# Since GetDefaultSpacesForLayout is now not used or used for layout, let's fix the logic in GenerateUnits.
# Instead of Regex replacement which is brittle, I'll replace the entire GenerateUnits method using C# script because of Turkish characters.
[System.IO.File]::WriteAllText($ctrlPath, $ctrlText, [System.Text.Encoding]::UTF8)
