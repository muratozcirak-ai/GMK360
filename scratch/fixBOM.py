import os

files_to_fix = [
    'GMK360.Web/Views/ConstructionProject/Details.cshtml',
    'GMK360.Web/Views/ConstructionProject/Amenities.cshtml'
]

for file_path in files_to_fix:
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    with open(file_path, 'w', encoding='utf-8-sig') as f:
        f.write(content)
        
    print(f"Fixed BOM for {file_path}")
