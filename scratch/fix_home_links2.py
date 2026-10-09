import codecs
import re

path = 'GMK360.Web/Views/Home/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('/Cozumler/Insaat-Proje', '/ConstructionProject/Index')
content = content.replace('/Cozumler/Gunluk-Kiralama', '/Dashboard/Corporate')
content = content.replace('/Cozumler/Emlak-Danisman', '/Dashboard/Corporate')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print("Updated all Cozumler links in Home/Index.cshtml")