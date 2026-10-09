with open("GMK360.Web/Controllers/ConstructionProjectController.cs", "r", encoding="utf-8") as f:
    content = f.read()

import re

# We need to flatten the blocks before processing!
flatten_logic = """
                var allBlocks = new System.Collections.Generic.List<GMK360.Web.Models.WizardBlockItem>();
                
                void AddBlocks(IEnumerable<GMK360.Web.Models.WizardBlockItem> blocks, bool isExisting, string parentId = null) {
                    if (blocks == null) return;
                    foreach(var b in blocks) {
                        b.IsExistingBuilding = isExisting;
                        allBlocks.Add(b);
                        if (b.SubBlocks != null && b.SubBlocks.Any()) {
                            // SubBlocks don't have their own IsExisting set by model binder usually, so inherit
                            AddBlocks(b.SubBlocks, isExisting, b.BlockName);
                        }
                    }
                }

                if (model.ExistingBlocks != null) {
                    AddBlocks(model.ExistingBlocks, true);
                }
                if (model.TargetBlocks != null) {
                    AddBlocks(model.TargetBlocks, false);
                }
"""

content = re.sub(r'var allBlocks = new System\.Collections\.Generic\.List<GMK360\.Web\.Models\.WizardBlockItem>\(\);\s*if \(model\.ExistingBlocks != null\) \{.*?(?=if \(allBlocks\.Any\(\)\))', flatten_logic, content, flags=re.DOTALL)

with open("GMK360.Web/Controllers/ConstructionProjectController.cs", "w", encoding="utf-8") as f:
    f.write(content)
print("Updated SaveStep2 flattening logic!")
