import re

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# We need to find the TapuDocumentFile upload block and remove the 'project.CoverImageUrl = archive.DocumentUrl;' line.
# Let's find it.
pattern = r'(if\s*\(model\.TapuDocumentFile\s*!=\s*null\s*&&\s*model\.TapuDocumentFile\.Length\s*>\s*0\).*?_context\.DocumentArchives\.Add\(archive\);\s*)project\.CoverImageUrl\s*=\s*archive\.DocumentUrl;'
match = re.search(pattern, content, flags=re.DOTALL)

if match:
    content = content[:match.end(1)] + content[match.end():]
    with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'w', encoding='utf-8') as f:
        f.write(content)
    print("Tapu document upload bug fixed!")
else:
    print("Could not find the pattern!")

