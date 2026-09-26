import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

pattern = r"if \(model\.Blocks != null\)\s*\{\s*var existingBlocks = project\.Blocks\?\.ToList\(\) \?\? new System\.Collections\.Generic\.List<GMK360\.Core\.Entities\.Building>\(\);\s*var currentBlockNames = model\.Blocks\.Select\(b => b\.BlockName\)\.ToList\(\);.*?_context\.Buildings\.Add\(newBlock\);\s*\}\s*\}\s*\}"

replacement = """
                var allIncomingBlocks = new List<GMK360.Web.Models.WizardBlockItem>();
                if (model.ExistingBlocks != null) allIncomingBlocks.AddRange(model.ExistingBlocks);
                if (model.TargetBlocks != null) allIncomingBlocks.AddRange(model.TargetBlocks);
                if (model.Blocks != null && !allIncomingBlocks.Any()) allIncomingBlocks.AddRange(model.Blocks);

                if (allIncomingBlocks.Any())
                {
                    var existingBlocks = project.Blocks?.ToList() ?? new System.Collections.Generic.List<GMK360.Core.Entities.Building>();
                    var currentBlockNames = allIncomingBlocks.Where(b => !string.IsNullOrEmpty(b.BlockName)).Select(b => b.BlockName).ToList();

                    // Sadece formda olmayan eski blokları sil
                    var blocksToRemove = existingBlocks.Where(b => !currentBlockNames.Contains(b.BlockName)).ToList();
                    if (blocksToRemove.Any()) {
                        _context.Buildings.RemoveRange(blocksToRemove);
                    }

                    foreach (var b in allIncomingBlocks)
                    {
                        var blockEntity = existingBlocks.FirstOrDefault(eb => eb.BlockName == b.BlockName);
                        if (blockEntity != null)
                        {
                            blockEntity.BaseArea = b.BaseArea;
                            blockEntity.TotalFloors = b.TotalFloors;
                            blockEntity.BasementFloors = b.BasementFloors;
                            blockEntity.TotalUnits = b.TotalApartments + b.TotalShops;
                            blockEntity.TotalApartments = b.TotalApartments;
                            blockEntity.TotalShops = b.TotalShops;
                            blockEntity.HasGroundFloor = b.HasGroundFloor;
                            blockEntity.HasRoof = b.HasRoof;
                            blockEntity.IsExistingBuilding = b.IsExistingBuilding;
                            blockEntity.LayoutPattern = b.LayoutPattern;
                        }
                        else
                        {
                            var newBlock = new GMK360.Core.Entities.Building
                            {
                                ConstructionProjectId = project.Id,
                                BlockName = b.BlockName,
                                Name = b.BlockName,
                                Type = "Bina",
                                BaseArea = b.BaseArea,
                                TotalFloors = b.TotalFloors,
                                BasementFloors = b.BasementFloors,
                                TotalUnits = b.TotalApartments + b.TotalShops,
                                TotalApartments = b.TotalApartments,
                                TotalShops = b.TotalShops,
                                HasGroundFloor = b.HasGroundFloor,
                                HasRoof = b.HasRoof,
                                IsExistingBuilding = b.IsExistingBuilding,
                                LayoutPattern = b.LayoutPattern
                            };
                            _context.Buildings.Add(newBlock);
                        }
                    }
                }
"""

content = re.sub(pattern, replacement, content, flags=re.DOTALL)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("SaveStep2 updated.")
