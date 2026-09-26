import sys

filepath = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with open(filepath, 'rb') as f:
    content = f.read()

# strip BOM
if content.startswith(b'\xef\xbb\xbf'):
    content = content[3:]

with open(filepath, 'wb') as f:
    f.write(content)
