import re

with open('GMK360.Web/Controllers/AdminLegalDocumentController.cs', 'r', encoding='utf-8') as f:
    text = f.read()

text = text.replace('""', '"')

with open('GMK360.Web/Controllers/AdminLegalDocumentController.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print("Quotes fixed")
