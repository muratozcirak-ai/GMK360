import codecs
import re
import os

os.makedirs('GMK360.Web/Views/PhaseTwo', exist_ok=True)
path = 'GMK360.Web/Views/PhaseOne/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace names
content = content.replace('/PhaseOne/', '/PhaseTwo/')
content = content.replace('FAZ 1: Yıkım ve Zemin Hazırlığı (Fizibilite Bütçesi)', 'FAZ 2: Hafriyat ve Temel İnşaatı (Kaba Yapı Başlangıcı)')

# Update SubCategories
target = r'var subCategories = new List<string> \{.*?\};'
replacement = '''var subCategories = new List<string> {
        "2.1 Hafriyat ve Zemin İksası (Destekleme)",
        "2.2 Temel Altı Hazırlık ve Yalıtım (Bohçalama)",
        "2.3 Temel Betonarme İmalatı"
    };'''
content = re.sub(target, replacement, content, flags=re.DOTALL)

with codecs.open('GMK360.Web/Views/PhaseTwo/Index.cshtml', 'w', 'utf-8-sig') as f:
    f.write(content)