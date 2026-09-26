import re

filepath = r'GMK360.Web\Controllers\AdminController.B2b.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Add AddedByUserId mapping
if "AddedByUserId =" not in content:
    content = content.replace("Rating = 0", "Rating = 0,\n                AddedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value")
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
