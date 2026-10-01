import io
import re

def clean_file(filepath, strings_to_remove):
    try:
        with io.open(filepath, 'r', encoding='utf-8') as f:
            lines = f.readlines()
        
        new_lines = []
        for line in lines:
            if any(s in line for s in strings_to_remove):
                continue
            new_lines.append(line)
            
        with io.open(filepath, 'w', encoding='utf-8') as f:
            f.writelines(new_lines)
    except Exception as e:
        pass

# InventoryTransaction.cs
clean_file(r'GMK360.Core\Entities\Construction\InventoryTransaction.cs', ['PhaseTaskId', 'PhaseTask'])

# CostCategory.cs
clean_file(r'GMK360.Core\Entities\Construction\CostCategory.cs', ['TaskCost', 'PhaseItemType'])

# ConstructionProject.cs
clean_file(r'GMK360.Core\Entities\Construction\ConstructionProject.cs', ['ProjectPhase'])

print("Cleaned additional dependencies")
