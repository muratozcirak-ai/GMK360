import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Let's find the Exact string using regex
pattern = r"if \(draft\.Blocks != null && draft\.Blocks\.Any\(\)\) \{\s*model\.Blocks = draft\.Blocks\.Select\(b => new GMK360\.Web\.Models\.WizardBlockItem \{[^}]+\}\)\.ToList\(\);\s*\}"

replacement = """if (draft.Blocks != null && draft.Blocks.Any()) {
                        model.ExistingBlocks = draft.Blocks.Where(b => b.IsExistingBuilding).Select(b => new GMK360.Web.Models.WizardBlockItem {
                            BlockName = b.BlockName,
                            BaseArea = b.BaseArea,
                            TotalFloors = b.TotalFloors ?? 0,
                            BasementFloors = b.BasementFloors,
                            TotalApartments = b.TotalApartments > 0 ? b.TotalApartments : b.TotalUnits,
                            TotalShops = b.TotalShops,
                            HasGroundFloor = b.HasGroundFloor,
                            HasRoof = b.HasRoof,
                            IsExistingBuilding = true,
                            LayoutPattern = b.LayoutPattern
                        }).ToList();

                        model.TargetBlocks = draft.Blocks.Where(b => !b.IsExistingBuilding).Select(b => new GMK360.Web.Models.WizardBlockItem {
                            BlockName = b.BlockName,
                            BaseArea = b.BaseArea,
                            TotalFloors = b.TotalFloors ?? 0,
                            BasementFloors = b.BasementFloors,
                            TotalApartments = b.TotalApartments > 0 ? b.TotalApartments : b.TotalUnits,
                            TotalShops = b.TotalShops,
                            HasGroundFloor = b.HasGroundFloor,
                            HasRoof = b.HasRoof,
                            IsExistingBuilding = false,
                            LayoutPattern = b.LayoutPattern
                        }).ToList();
                    }"""

content = re.sub(pattern, replacement, content)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("GET Method Blocks mapping updated.")
