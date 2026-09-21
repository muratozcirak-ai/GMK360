import re

with open(r"Controllers\ConstructionProjectController.cs", "r", encoding="utf-8") as f:
    content = f.read()

find_str = """                            existing.TechnicalFeatures = b.StructureType;
                            existing.Description = b.ParentIndex;"""
                            
replace_str = """                            existing.TechnicalFeatures = b.StructureType;
                            existing.Description = b.ParentIndex;
                            existing.LayoutPattern = b.LayoutPattern;
                            existing.AttachedToBlock = b.AttachedToBlock;"""
content = content.replace(find_str, replace_str)

find_str_2 = """                            var newBlock = new GMK360.Core.Entities.Building
                            {
                                Name = string.IsNullOrWhiteSpace(b.BlockName) ? "İsimsiz Blok" : b.BlockName,
                                BlockName = b.BlockName,
                                BaseArea = b.BaseArea,
                                TotalFloors = b.TotalFloors,
                                TotalApartments = b.TotalApartments,
                                TotalShops = b.TotalShops,
                                BasementFloors = b.BasementFloors,
                                HasGroundFloor = b.HasGroundFloor,
                                HasRoof = b.HasRoof,
                                TechnicalFeatures = b.StructureType,
                                Description = b.ParentIndex
                            };"""
                            
replace_str_2 = """                            var newBlock = new GMK360.Core.Entities.Building
                            {
                                Name = string.IsNullOrWhiteSpace(b.BlockName) ? "İsimsiz Blok" : b.BlockName,
                                BlockName = b.BlockName,
                                BaseArea = b.BaseArea,
                                TotalFloors = b.TotalFloors,
                                TotalApartments = b.TotalApartments,
                                TotalShops = b.TotalShops,
                                BasementFloors = b.BasementFloors,
                                HasGroundFloor = b.HasGroundFloor,
                                HasRoof = b.HasRoof,
                                TechnicalFeatures = b.StructureType,
                                Description = b.ParentIndex,
                                LayoutPattern = b.LayoutPattern,
                                AttachedToBlock = b.AttachedToBlock
                            };"""
content = content.replace(find_str_2, replace_str_2)

with open(r"Controllers\ConstructionProjectController.cs", "w", encoding="utf-8") as f:
    f.write(content)
print("Updated Controller SaveStep2")
