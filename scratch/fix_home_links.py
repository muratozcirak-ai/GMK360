import codecs
import re

path = 'GMK360.Web/Views/Home/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Update Bireysel Kiraci
content = content.replace('/Cozumler/Bireysel-Kiraci', '/Dashboard/PropertyOwner')
# Update Bina Site Yonetimi
content = content.replace('/Cozumler/Bina-Site-Yonetimi', '/BuildingManager/Detail/1')
# Update Kurumsal Emlak
content = content.replace('/Cozumler/Kurumsal-Emlak', '/Dashboard/Corporate')
# Update Insaat ERP
content = content.replace('/Cozumler/Insaat-ERP', '/Dashboard/Construction')
# Update Esnaf Hizmet (assuming it exists, let's just regex all /Cozumler)
content = re.sub(r'/Cozumler/Esnaf-Hizmet', '/Dashboard/ServiceProvider', content)
content = re.sub(r'/Cozumler/Santiye-Tedarik', '/Dashboard/Supplier', content)
content = re.sub(r'/Cozumler/E-Ticaret', '/Dashboard/Hub', content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print("Updated Home/Index.cshtml links")