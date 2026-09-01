import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

create_wizard_action = """
        // WIZARD CREATE ACTION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWizard(GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            
            var project = new GMK360.Core.Entities.Construction.ConstructionProject
            {
                Name = model.Name,
                Description = model.Description,
                Address = model.Address,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CoverImageUrl = model.CoverImageUrl,
                AgencyId = agencyId.Value,
                Status = 0,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.ConstructionProjects.Add(project);
            await _context.SaveChangesAsync();
            
            if (model.Blocks != null && model.Blocks.Count > 0)
            {
                foreach (var b in model.Blocks)
                {
                    var building = new GMK360.Core.Entities.Building
                    {
                        Name = project.Name + " - " + b.BlockName,
                        BlockName = b.BlockName,
                        HasBlock = true,
                        TotalFloors = b.TotalFloors,
                        TotalUnits = b.TotalApartments + b.TotalShops,
                        ConstructionProjectId = project.Id,
                        CreatedAt = DateTime.UtcNow,
                        ManagerUserId = _userManager.GetUserId(User)
                    };
                    _context.Buildings.Add(building);
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
"""
if "CreateWizard" not in content:
    # Insert before the traditional Create POST
    content = content.replace("public async Task<IActionResult> Create([Bind", create_wizard_action + "\n        public async Task<IActionResult> Create([Bind")
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Injected CreateWizard.")
else:
    print("Already injected.")
