import re
import os

# 1. Update BuildingUnit.cs
file_unit = r"C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\BuildingUnit.cs"
with open(file_unit, 'r', encoding='utf-8') as f:
    content_unit = f.read()

new_unit_fields = """        public string DoorNumber { get; set; } // Örn: "Daire 1", "Dükkan 3"
        
        public int FloorLevel { get; set; } = 1; // 0: Zemin, -1: 1. Bodrum vs.
        public string? FloorName { get; set; } // Örn: "Zemin Kat", "Otopark"
"""
content_unit = content_unit.replace('        public string DoorNumber { get; set; } // Ã–rn: "Daire 1", "DÃ¼kkan 3"', new_unit_fields)

# If regex replace failed due to encoding weirdness, let's try a safer replace
if "FloorLevel" not in content_unit:
    content_unit = re.sub(
        r'public string DoorNumber \{ get; set; \}.*?\n',
        r'public string DoorNumber { get; set; }\n        public int FloorLevel { get; set; } = 1;\n        public string? FloorName { get; set; }\n',
        content_unit
    )

with open(file_unit, 'w', encoding='utf-8') as f:
    f.write(content_unit)


# 2. Update Building.cs
file_building = r"C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Building.cs"
with open(file_building, 'r', encoding='utf-8') as f:
    content_building = f.read()

new_building_fields = """        public int? TotalFloors { get; set; } // Binadaki Toplam Normal Kat Sayısı
        public int BasementFloors { get; set; } = 0; // Bodrum kat sayısı (Örn: 2)
        public bool HasGroundFloor { get; set; } = true; // Zemin kat var mı?
"""

content_building = re.sub(
    r'public int\? TotalFloors \{ get; set; \}.*?\n',
    new_building_fields,
    content_building
)

with open(file_building, 'w', encoding='utf-8') as f:
    f.write(content_building)

print("Updated BuildingUnit.cs and Building.cs")
