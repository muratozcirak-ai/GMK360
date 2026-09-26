import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Remove the explicit Update() calls because EF Core's ChangeTracker handles it for tracked entities.
# Specifically in the Details action:
content = content.replace("_context.ProjectLegalDocuments.Update(existingMain);", "// _context.ProjectLegalDocuments.Update(existingMain); EF ChangeTracker hallediyor")
content = content.replace("_context.ProjectLegalDocuments.Update(existingPr);", "// _context.ProjectLegalDocuments.Update(existingPr); EF ChangeTracker hallediyor")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
