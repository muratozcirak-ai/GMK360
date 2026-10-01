import io

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("Faz 0\\' Ynet", "Faz 0'ı Yönet")
content = content.replace("Faz 0\\'ı Yönet", "Faz 0'ı Yönet")

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
