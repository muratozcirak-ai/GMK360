import re

filepath = r'GMK360.Web\Views\Admin\B2bList.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('@cat.Category.Name', '@(cat.DefinitionValue?.Name ?? "-")')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Fixed B2bList.cshtml error.")
