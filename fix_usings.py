import re
import os

files_to_fix = [
    r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\CustomerPortalController.cs",
    r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ProjectMaterialController.cs"
]

for filepath in files_to_fix:
    if os.path.exists(filepath):
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
        
        content = content.replace('using GMK360.Data;', 'using GMK360.Data;\nusing GMK360.Data.Contexts;')
        
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)

print("Fixed using statements.")
