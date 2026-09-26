import sys

filepath = 'GMK360.Web/Controllers/ModuleDocumentRuleController.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = ".ThenBy(r => r.Stage)"
replacement = ""

content = content.replace(target, replacement)
with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
