import re

filepath = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

pattern = r'@\(doc\.SystemTemplate\?\.TargetModule \?\? "Projeye Özel eklenmiş evrak\."\)'
replacement = '''@(doc.SystemTemplate?.IssuedBy ?? "Projeye Özel eklenmiş evrak.")'''

content = re.sub(pattern, replacement, content)

with open(filepath, 'w', encoding='utf-8-sig') as f:
    f.write(content)
