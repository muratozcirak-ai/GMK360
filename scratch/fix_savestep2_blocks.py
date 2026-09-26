import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

pattern = r'(if\s*\(model\.Blocks\s*!=\s*null\)\s*\{).*?(await\s*_context\.SaveChangesAsync\(\);\s*return Json\(new \{ success = true \}\);\s*\})'

replacement = r"""var allBlocks = new System.Collections.Generic.List<GMK360.Web.Models.WizardBlockItem>();
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
                    var currentBlockNames = allBlocks.Select(b => b.BlockName).ToList();

                    // Sadece formda olmayan bloklar sil
                    var blocksToRemove = existingBlocks.Where(b => !currentBlockNames.Contains(b.BlockName)).ToList();
                    if (blocksToRemove.Any()) {
                        _context.Buildings.RemoveRange(blocksToRemove);
                    }

                    int blockCounter = 1;
                    foreach (var b in allBlocks)
                    {
                        var existingBlock = existingBlocks.FirstOrDefault(eb => eb.BlockName == b.BlockName && eb.IsExistingBuilding == b.IsExistingBuilding);
                        if (existingBlock != null)
                        {
                            // Varsa SADECE GÜNCELLE
                            existingBlock.BaseArea = b.BaseArea;
                            existingBlock.BasementFloors = b.BasementFloors;
                            existingBlock.TotalFloors = b.TotalFloors;
                            existingBlock.TotalUnits = b.TotalApartments + b.TotalShops;
                            existingBlock.TotalApartments = b.TotalApartments;
                            existingBlock.TotalShops = b.TotalShops;
                            existingBlock.HasRoof = b.HasRoof;
                            existingBlock.HasGroundFloor = b.HasGroundFloor;
                            existingBlock.LayoutPattern = b.LayoutPattern;
                            existingBlock.BuildingAge = b.BuildingAge;
                        }
                        else
                        {
                            // Yoksa YENİ EKLE
                            var building = new GMK360.Core.Entities.Building
                            {
                                Name = project.Name + " - " + b.BlockName,
                                BlockName = b.BlockName,
                                BuildingNumber = blockCounter.ToString(),
                                StreetName = "Belirtilmedi",
                                BaseArea = b.BaseArea,
                                BasementFloors = b.BasementFloors,
                                TotalFloors = b.TotalFloors,
                                TotalUnits = b.TotalApartments + b.TotalShops,
                                TotalApartments = b.TotalApartments,
                                TotalShops = b.TotalShops,
                                HasBlock = true,
                                HasRoof = b.HasRoof,
                                HasGroundFloor = b.HasGroundFloor,
                                IsExistingBuilding = b.IsExistingBuilding,
                                LayoutPattern = b.LayoutPattern,
                                BuildingAge = b.BuildingAge,
                                ConstructionProjectId = project.Id,
                                CityId = project.CityId ?? 34,
                                DistrictId = project.DistrictId ?? 1,
                                NeighborhoodId = project.NeighborhoodId ?? 1,
                                StreetId = project.StreetId,
                                CreatedAt = DateTime.UtcNow,
                                ManagerUserId = _userManager.GetUserId(User) ?? ""
                            };
                            _context.Buildings.Add(building);
                        }
                        blockCounter++;
                    }
                }

                \2"""

new_content = re.sub(pattern, replacement, content, flags=re.DOTALL)
with open(filepath, 'w', encoding='utf-8') as f:
    f.write(new_content)

print("SaveStep2 updated to use ExistingBlocks and TargetBlocks.")
