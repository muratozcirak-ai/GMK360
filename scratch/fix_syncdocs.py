import io
import re

filepath = r'GMK360.Web\Controllers\PhaseZeroController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Find the InstitutionContact line in SyncDocs
target = r'InstitutionContact = rule\.SystemLegalDocumentTemplate\.IssuedBy,'
replacement = r'InstitutionContact = null,'
content = re.sub(target, replacement, content)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated PhaseZeroController.cs")
