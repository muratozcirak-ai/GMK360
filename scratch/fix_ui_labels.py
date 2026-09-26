import re

filepath = r'GMK360.Web\Views\ConstructionProject\Create.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace in Server Loop and JS Template
content = content.replace('Bina Yaşı (Yıl)', 'Yapım Yılı')
content = content.replace('Örn: 25', 'Örn: 1998')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("UI labels updated.")
