import re

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'r', encoding='utf-8') as f:
    code = f.read()

# Fix SaveStep2 overwrite bug:
# We need to change existingBlocks.FirstOrDefault(eb => eb.BlockName == b.BlockName)
# to existingBlocks.FirstOrDefault(eb => eb.BlockName == b.BlockName && eb.IsExistingBuilding == b.IsExistingBuilding)

code = code.replace(
    'existingBlocks.FirstOrDefault(eb => eb.BlockName == b.BlockName)',
    'existingBlocks.FirstOrDefault(eb => eb.BlockName == b.BlockName && eb.IsExistingBuilding == b.IsExistingBuilding)'
)

# Fix blocksToRemove
# Wait, let's just find the SaveStep2 block logic.
