$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
$text = [System.IO.File]::ReadAllText($path)

$action = @"
        [HttpPost]
        public async Task<IActionResult> UpdateProjectLandArea(int projectId, double? totalLandArea, double? landscapeArea)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project != null)
            {
                project.TotalLandArea = totalLandArea;
                project.LandscapeArea = landscapeArea;
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Amenities), new { projectId = projectId });
        }
"@

$text = $text.Replace("public async Task<IActionResult> Amenities(int projectId)", $action + "`r`n        public async Task<IActionResult> Amenities(int projectId)")

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
