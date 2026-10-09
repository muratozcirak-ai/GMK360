with open("GMK360.Web/Controllers/ConstructionProjectController.cs", "r", encoding="utf-8") as f:
    content = f.read()

import re

new_savestep2 = """        public async Task<IActionResult> SaveStep2([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            try {
                if (model.DraftProjectId == 0) return Json(new { success = false, message = "Proje ID bulunamadı." });
                var project = await _context.ConstructionProjects.Include(p => p.Blocks).FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
                if (project == null) return Json(new { success = false, message = "Proje bulunamadı." });

                var existingBlocks = project.Blocks?.ToList() ?? new System.Collections.Generic.List<GMK360.Core.Entities.Building>();
                var parentBlocks = new System.Collections.Generic.List<GMK360.Web.Models.WizardBlockItem>();
                if (model.ExistingBlocks != null) { foreach(var b in model.ExistingBlocks) { b.IsExistingBuilding = true; parentBlocks.Add(b); } }
                if (model.TargetBlocks != null) { foreach(var b in model.TargetBlocks) { b.IsExistingBuilding = false; parentBlocks.Add(b); } }

                var allCurrentNames = new System.Collections.Generic.List<string>();
                
                int blockCounter = 1;
                foreach (var pb in parentBlocks) {
                    allCurrentNames.Add(pb.BlockName);
                    var existingParent = existingBlocks.FirstOrDefault(eb => eb.BlockName == pb.BlockName && eb.IsExistingBuilding == pb.IsExistingBuilding && eb.ParentBuildingId == null);
                    
                    if (existingParent != null) {
                        existingParent.BaseArea = pb.BaseArea;
                        existingParent.BasementFloors = pb.BasementFloors;
                        existingParent.TotalFloors = pb.TotalFloors;
                        existingParent.TotalUnits = pb.TotalApartments + pb.TotalShops;
                        existingParent.TotalApartments = pb.TotalApartments;
                        existingParent.TotalShops = pb.TotalShops;
                        existingParent.HasGroundFloor = pb.HasGroundFloor;
                        existingParent.HasRoof = pb.HasRoof;
                        existingParent.BuildingAge = pb.BuildingAge;
                        existingParent.LayoutPattern = pb.LayoutPattern;
                    } else {
                        existingParent = new GMK360.Core.Entities.Building {
                            Name = project.Name + " - " + pb.BlockName,
                            BlockName = pb.BlockName,
                            BuildingNumber = blockCounter.ToString(),
                            StreetName = "Belirtilmedi",
                            IsExistingBuilding = pb.IsExistingBuilding,
                            BaseArea = pb.BaseArea,
                            BasementFloors = pb.BasementFloors,
                            TotalFloors = pb.TotalFloors,
                            TotalUnits = pb.TotalApartments + pb.TotalShops,
                            TotalApartments = pb.TotalApartments,
                            TotalShops = pb.TotalShops,
                            HasGroundFloor = pb.HasGroundFloor,
                            HasRoof = pb.HasRoof,
                            BuildingAge = pb.BuildingAge,
                            LayoutPattern = pb.LayoutPattern,
                            ConstructionProjectId = project.Id,
                            CityId = project.CityId ?? 34,
                            DistrictId = project.DistrictId ?? 1,
                            NeighborhoodId = project.NeighborhoodId ?? 1,
                            StreetId = project.StreetId,
                            CreatedAt = DateTime.UtcNow,
                            ManagerUserId = _userManager.GetUserId(User) ?? ""
                        };
                        _context.Buildings.Add(existingParent);
                        existingBlocks.Add(existingParent); // Add to local list for child linking
                    }
                    
                    // İşlem bittikten sonra Parent ID almak için kaydetmemiz gerekmiyor çünkü Entity Framework navigasyon üzerinden id çözer
                    if (pb.SubBlocks != null && pb.SubBlocks.Any()) {
                        foreach (var sb in pb.SubBlocks) {
                            allCurrentNames.Add(sb.BlockName);
                            // Ebeveyn bağlantısını kontrol et.
                            var existingChild = existingBlocks.FirstOrDefault(eb => eb.BlockName == sb.BlockName && eb.IsExistingBuilding == pb.IsExistingBuilding && (eb.ParentBuilding == existingParent || eb.ParentBuildingId == existingParent.Id));
                            
                            if (existingChild != null) {
                                existingChild.TotalFloors = sb.TotalFloors;
                                existingChild.TotalUnits = sb.TotalApartments + sb.TotalShops;
                                existingChild.TotalApartments = sb.TotalApartments;
                                existingChild.TotalShops = sb.TotalShops;
                                existingChild.BasementFloors = sb.BasementFloors;
                                existingChild.HasGroundFloor = sb.HasGroundFloor;
                                existingChild.HasRoof = sb.HasRoof;
                            } else {
                                existingChild = new GMK360.Core.Entities.Building {
                                    Name = project.Name + " - " + sb.BlockName,
                                    BlockName = sb.BlockName,
                                    BuildingNumber = blockCounter.ToString() + "-Sub",
                                    StreetName = "Belirtilmedi",
                                    IsExistingBuilding = pb.IsExistingBuilding,
                                    TotalFloors = sb.TotalFloors,
                                    TotalUnits = sb.TotalApartments + sb.TotalShops,
                                    TotalApartments = sb.TotalApartments,
                                    TotalShops = sb.TotalShops,
                                    BasementFloors = sb.BasementFloors,
                                    HasGroundFloor = sb.HasGroundFloor,
                                    HasRoof = sb.HasRoof,
                                    ConstructionProjectId = project.Id,
                                    ParentBuilding = existingParent, // Navigation mapping
                                    CityId = project.CityId ?? 34,
                                    DistrictId = project.DistrictId ?? 1,
                                    NeighborhoodId = project.NeighborhoodId ?? 1,
                                    StreetId = project.StreetId,
                                    CreatedAt = DateTime.UtcNow,
                                    ManagerUserId = _userManager.GetUserId(User) ?? ""
                                };
                                _context.Buildings.Add(existingChild);
                                existingBlocks.Add(existingChild);
                            }
                        }
                    }
                    blockCounter++;
                }
                
                // Silme işlemi
                var blocksToRemove = existingBlocks.Where(b => !allCurrentNames.Contains(b.BlockName)).ToList();
                if (blocksToRemove.Any()) {
                    _context.Buildings.RemoveRange(blocksToRemove);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });

            } catch (Exception ex) {
                return Json(new { success = false, message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "") });
            }
        }"""

content = re.sub(r'public async Task<IActionResult> SaveStep2.*?return Json\(new \{ success = false, message = ex.Message \+ \(ex.InnerException != null \? " - " \+ ex.InnerException.Message : ""\) \}\);\s*\}\s*\}', new_savestep2, content, flags=re.DOTALL)

with open("GMK360.Web/Controllers/ConstructionProjectController.cs", "w", encoding="utf-8") as f:
    f.write(content)
print("Updated SaveStep2 fully!")
