import sys

filepath = 'GMK360.Web/Controllers/AdminController.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = ".ThenInclude(cc => cc.B2bCategory)"
replacement = ".ThenInclude(cc => cc.DefinitionValue)"

content = content.replace(target, replacement)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
