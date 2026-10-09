import re

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# We'll locate SaveStep2 block
start_str = 'public async Task<IActionResult> SaveStep2([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)'
end_str = 'return Json(new { success = true, draftId = project.Id, projectId = project.Id });'

start_idx = content.find(start_str)
end_idx = content.find(end_str, start_idx) + len(end_str)

new_code = '''public async Task<IActionResult> SaveStep2([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            try {
                if (model.DraftProjectId == 0) return Json(new { success = false, message = "Proje ID bulunamadı." });
                var project = await _context.ConstructionProjects.Include(p => p.Blocks).ThenInclude(b => b.ChildBuildings).FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
                if (project == null) return Json(new { success = false, message = "Proje bulunamadı." });

                var allBlocks = new System.Collections.Generic.List<GMK360.Web.Models.WizardBlockItem>();
                if (model.ExistingBlocks != null) {
                    foreach(var eb in model.ExistingBlocks) {
                        eb.IsExistingBuilding = true;
                        allBlocks.Add(eb);
                    }
                }
                if (model.TargetBlocks != null) {
                    foreach(var tb in model.TargetBlocks) {
                        tb.IsExistingBuilding = false;
                        allBlocks.Add(tb);
                    }
                }

                if (allBlocks.Any())
                {
                    var existingBlocks = project.Blocks?.ToList() ?? new System.Collections.Generic.List<GMK360.Core.Entities.Building>();
                    
                    // Recursive update/insert logic for main blocks
                    foreach (var b in allBlocks)
                    {
                        var mainBlock = existingBlocks.FirstOrDefault(eb => eb.BlockName == b.BlockName && eb.IsExistingBuilding == b.IsExistingBuilding && eb.ParentBuildingId == null);
                        
                        if (mainBlock != null)
                        {
                            mainBlock.BaseArea = b.BaseArea;
                            mainBlock.BasementFloors = b.BasementFloors;
                            mainBlock.TotalFloors = b.TotalFloors;
                            mainBlock.TotalUnits = b.TotalApartments + b.TotalShops;
                            mainBlock.TotalApartments = b.TotalApartments;
                            mainBlock.TotalShops = b.TotalShops;
                            mainBlock.HasRoof = b.HasRoof;
                            mainBlock.HasGroundFloor = b.HasGroundFloor;
                            mainBlock.LayoutPattern = b.LayoutPattern;
                            mainBlock.BuildingAge = b.BuildingAge;
                        }
                        else
                        {
                            mainBlock = new GMK360.Core.Entities.Building
                            {
                                Name = project.Name + " - " + b.BlockName,
                                BlockName = b.BlockName,
                                BaseArea = b.BaseArea,
                                BasementFloors = b.BasementFloors,
                                TotalFloors = b.TotalFloors,
                                TotalUnits = b.TotalApartments + b.TotalShops,
                                TotalApartments = b.TotalApartments,
                                TotalShops = b.TotalShops,
                                HasRoof = b.HasRoof,
                                HasGroundFloor = b.HasGroundFloor,
                                ConstructionProjectId = project.Id,
                                IsExistingBuilding = b.IsExistingBuilding,
                                LayoutPattern = b.LayoutPattern,
                                BuildingAge = b.BuildingAge
                            };
                            _context.Buildings.Add(mainBlock);
                            existingBlocks.Add(mainBlock); // so we can add children to it later
                        }
                        
                        // We need to SaveChanges here so mainBlock gets an ID if it's new
                        await _context.SaveChangesAsync();
                        
                        // Now handle SubBlocks (Towers)
                        if (b.SubBlocks != null && b.SubBlocks.Any())
                        {
                            var existingChildren = mainBlock.ChildBuildings?.ToList() ?? new System.Collections.Generic.List<GMK360.Core.Entities.Building>();
                            
                            foreach(var sub in b.SubBlocks)
                            {
                                var childBlock = existingChildren.FirstOrDefault(c => c.BlockName == sub.BlockName);
                                if (childBlock != null)
                                {
                                    childBlock.TotalFloors = sub.TotalFloors;
                                    childBlock.TotalApartments = sub.TotalApartments;
                                    childBlock.TotalShops = sub.TotalShops;
                                    childBlock.BasementFloors = sub.BasementFloors;
                                    childBlock.HasGroundFloor = sub.HasGroundFloor;
                                    childBlock.HasRoof = sub.HasRoof;
                                    childBlock.TotalUnits = sub.TotalApartments + sub.TotalShops;
                                }
                                else
                                {
                                    childBlock = new GMK360.Core.Entities.Building
                                    {
                                        Name = project.Name + " - " + mainBlock.BlockName + " - " + sub.BlockName,
                                        BlockName = sub.BlockName,
                                        ConstructionProjectId = project.Id,
                                        ParentBuildingId = mainBlock.Id,
                                        IsExistingBuilding = b.IsExistingBuilding,
                                        TotalFloors = sub.TotalFloors,
                                        TotalApartments = sub.TotalApartments,
                                        TotalShops = sub.TotalShops,
                                        BasementFloors = sub.BasementFloors,
                                        HasGroundFloor = sub.HasGroundFloor,
                                        HasRoof = sub.HasRoof,
                                        TotalUnits = sub.TotalApartments + sub.TotalShops
                                    };
                                    _context.Buildings.Add(childBlock);
                                }
                            }
                        }
                    }
                }
                
                await _context.SaveChangesAsync();
                return Json(new { success = true, draftId = project.Id, projectId = project.Id });
'''

content = content[:start_idx] + new_code + content[end_idx:]

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'w', encoding='utf-8') as f:
    f.write(content)
print("SaveStep2 updated!")
