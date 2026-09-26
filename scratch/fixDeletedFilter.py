import re

with open('GMK360.Web/Controllers/AdminLegalDocumentController.cs', 'r', encoding='utf-8') as f:
    text = f.read()

pattern = r'_context\.SystemLegalDocumentTemplates\.OrderBy'
replacement = r'_context.SystemLegalDocumentTemplates.Where(t => !t.IsDeleted).OrderBy'

if re.search(pattern, text):
    text = re.sub(pattern, replacement, text)
    with open('GMK360.Web/Controllers/AdminLegalDocumentController.cs', 'w', encoding='utf-8-sig') as f:
        f.write(text)
    print("Replaced successfully")
else:
    print("Pattern not found")
