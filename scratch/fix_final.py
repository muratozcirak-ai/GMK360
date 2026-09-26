import sys

filepath = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("!existingDocs.Contains(rule.SystemLegalDocumentTemplateId)", "!existingDocs.Contains((int?)rule.SystemLegalDocumentTemplateId)")

with open(filepath, 'w', encoding='utf-8-sig') as f:
    f.write(content)

filepath_v = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with open(filepath_v, 'r', encoding='utf-8') as f:
    content_v = f.read()

content_v = content_v.replace("doc.SystemTemplate?.TargetModule", "doc.SystemTemplate?.IssuedBy")

with open(filepath_v, 'w', encoding='utf-8') as f:
    f.write(content_v)
