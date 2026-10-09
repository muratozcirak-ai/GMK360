with open("GMK360.Web/Controllers/ConstructionProjectController.cs", "r", encoding="utf-8") as f:
    content = f.read()

import re

# 1. Update Create GET action to properly map SubBlocks!
def replace_get(match):
    return """if (draft.Blocks != null && draft.Blocks.Any()) {
                        model.ExistingBlocks = draft.Blocks.Where(b => b.IsExistingBuilding && b.ParentBuildingId == null).Select(b => new GMK360.Web.Models.WizardBlockItem {
                            BlockName = b.BlockName,
                            BaseArea = b.BaseArea,
                            TotalFloors = b.TotalFloors ?? 0,
                            TotalApartments = b.TotalApartments,
                            TotalShops = b.TotalShops,
                            BuildingAge = b.BuildingAge,
                            BasementFloors = b.BasementFloors,
                            HasGroundFloor = b.HasGroundFloor,
                            HasRoof = b.HasRoof,
                            LayoutPattern = b.LayoutPattern,
                            SubBlocks = draft.Blocks.Where(cb => cb.ParentBuildingId == b.Id).Select(cb => new GMK360.Web.Models.WizardBlockItem {
                                BlockName = cb.BlockName,
                                TotalFloors = cb.TotalFloors ?? 0,
                                TotalApartments = cb.TotalApartments,
                                TotalShops = cb.TotalShops,
                                BasementFloors = cb.BasementFloors,
                                HasGroundFloor = cb.HasGroundFloor,
                                HasRoof = cb.HasRoof
                            }).ToList()
                        }).ToList();
                        model.TargetBlocks = draft.Blocks.Where(b => !b.IsExistingBuilding && b.ParentBuildingId == null).Select(b => new GMK360.Web.Models.WizardBlockItem {
                            BlockName = b.BlockName,
                            BaseArea = b.BaseArea,
                            TotalFloors = b.TotalFloors ?? 0,
                            TotalApartments = b.TotalApartments,
                            TotalShops = b.TotalShops,
                            BuildingAge = b.BuildingAge,
                            BasementFloors = b.BasementFloors,
                            HasGroundFloor = b.HasGroundFloor,
                            HasRoof = b.HasRoof,
                            LayoutPattern = b.LayoutPattern,
                            SubBlocks = draft.Blocks.Where(cb => cb.ParentBuildingId == b.Id).Select(cb => new GMK360.Web.Models.WizardBlockItem {
                                BlockName = cb.BlockName,
                                TotalFloors = cb.TotalFloors ?? 0,
                                TotalApartments = cb.TotalApartments,
                                TotalShops = cb.TotalShops,
                                BasementFloors = cb.BasementFloors,
                                HasGroundFloor = cb.HasGroundFloor,
                                HasRoof = cb.HasRoof
                            }).ToList()
                        }).ToList();
                    }"""

content = re.sub(r'if \(draft\.Blocks != null && draft\.Blocks\.Any\(\)\) \{.*?model\.TargetBlocks = .*?\}\)\.ToList\(\);\s*\}', replace_get, content, flags=re.DOTALL)


# 2. Complete rewrite of SaveStep2 Blocks portion
def replace_savestep2(match):
    return """
                    var existingBlocks = project.Blocks?.ToList() ?? new System.Collections.Generic.List<GMK360.Core.Entities.Building>();
                    var parentBlocks = new System.Collections.Generic.List<GMK360.Web.Models.WizardBlockItem>();
                    if (model.ExistingBlocks != null) { foreach(var b in model.ExistingBlocks) { b.IsExistingBuilding = true; parentBlocks.Add(b); } }
                    if (model.TargetBlocks != null) { foreach(var b in model.TargetBlocks) { b.IsExistingBuilding = false; parentBlocks.Add(b); } }

                    var allCurrentNames = new System.Collections.Generic.List<string>();
                    
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
                                Name = pb.BlockName,
                                BlockName = pb.BlockName,
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
                                CityId = project.CityId ?? 1,
                                DistrictId = project.DistrictId ?? 1,
                                NeighborhoodId = project.NeighborhoodId ?? 1
                            };
                            _context.Buildings.Add(existingParent);
                            existingBlocks.Add(existingParent);
                        }
                        
                        // Alt Blokları İşle
                        if (pb.SubBlocks != null && pb.SubBlocks.Any()) {
                            foreach (var sb in pb.SubBlocks) {
                                allCurrentNames.Add(sb.BlockName);
                                var existingChild = existingBlocks.FirstOrDefault(eb => eb.BlockName == sb.BlockName && eb.IsExistingBuilding == pb.IsExistingBuilding && eb.ParentBuilding == existingParent);
                                if (existingChild == null && existingParent.Id > 0) {
                                    existingChild = existingBlocks.FirstOrDefault(eb => eb.BlockName == sb.BlockName && eb.IsExistingBuilding == pb.IsExistingBuilding && eb.ParentBuildingId == existingParent.Id);
                                }
                                
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
                                        Name = sb.BlockName,
                                        BlockName = sb.BlockName,
                                        IsExistingBuilding = pb.IsExistingBuilding,
                                        TotalFloors = sb.TotalFloors,
                                        TotalUnits = sb.TotalApartments + sb.TotalShops,
                                        TotalApartments = sb.TotalApartments,
                                        TotalShops = sb.TotalShops,
                                        BasementFloors = sb.BasementFloors,
                                        HasGroundFloor = sb.HasGroundFloor,
                                        HasRoof = sb.HasRoof,
                                        ConstructionProjectId = project.Id,
                                        ParentBuilding = existingParent,
                                        CityId = project.CityId ?? 1,
                                        DistrictId = project.DistrictId ?? 1,
                                        NeighborhoodId = project.NeighborhoodId ?? 1
                                    };
                                    _context.Buildings.Add(existingChild);
                                    existingBlocks.Add(existingChild);
                                }
                            }
                        }
                    }
                    
                    // Sadece formda olmayan blokları sil (Hem ana hem alt)
                    var blocksToRemove = existingBlocks.Where(b => !allCurrentNames.Contains(b.BlockName)).ToList();
                    if (blocksToRemove.Any()) {
                        _context.Buildings.RemoveRange(blocksToRemove);
                    }
"""

content = re.sub(r'var allBlocks = new System\.Collections\.Generic\.List<GMK360\.Web\.Models\.WizardBlockItem>\(\);.*?if \(blocksToRemove\.Any\(\)\) \{\s*_context\.Buildings\.RemoveRange\(blocksToRemove\);\s*\}\s*\}', replace_savestep2, content, flags=re.DOTALL)

with open("GMK360.Web/Controllers/ConstructionProjectController.cs", "w", encoding="utf-8") as f:
    f.write(content)
print("Updated DB Hierarchy successfully!")
