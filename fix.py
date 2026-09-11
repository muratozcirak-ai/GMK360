import sys

file_path = 'GMK360.Web/Views/ConstructionProject/ManageBlock.cshtml'
with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('ViewBag.FloorPlanıs', 'ViewBag.FloorPlans')
content = content.replace('floorPlanıs', 'floorPlans')

content = content.replace('Ä°skeletini', 'İskeletini')
content = content.replace('GÃ¼ncelle', 'Güncelle')
content = content.replace('Ã‡Ä±kar', 'Çıkar')
content = content.replace('Åžablon', 'Şablon')

start_idx = content.find('<!-- Mimari Dış Cephe Çizimler Paneli -->')
end_idx = content.find('<div class="col-md-8">')

if start_idx != -1 and end_idx != -1:
    content = content[:start_idx] + '\n    </div>\n    \n    ' + content[end_idx:]

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)
print('Done!')
